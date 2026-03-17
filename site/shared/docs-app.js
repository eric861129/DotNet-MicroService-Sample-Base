(() => {
  const manifest = window.DOCS_MANIFEST;

  if (!manifest || !window.marked) {
    return;
  }

  const markdownRenderer = window.marked;
  const contentCache = new Map();

  function slugify(text) {
    return text
      .toLowerCase()
      .trim()
      .replace(/[^\w\u4e00-\u9fff\s-]/g, "")
      .replace(/\s+/g, "-");
  }

  function escapeHtml(text) {
    return text
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#39;");
  }

  function escapeRegExp(text) {
    return text.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
  }

  function highlightText(text, keyword) {
    if (!keyword) {
      return escapeHtml(text);
    }

    const pattern = new RegExp(`(${escapeRegExp(keyword)})`, "ig");
    return escapeHtml(text).replace(pattern, "<mark>$1</mark>");
  }

  function getDocByPath(docPath) {
    return manifest.docs.find((doc) => doc.path === docPath) ?? manifest.docs[0];
  }

  function getDocById(docId) {
    return manifest.docs.find((doc) => doc.id === docId);
  }

  function groupDocs(docs) {
    return docs.reduce((groups, doc) => {
      if (!groups[doc.category]) {
        groups[doc.category] = [];
      }

      groups[doc.category].push(doc);
      return groups;
    }, {});
  }

  function buildDocUrl(docPath) {
    const url = new URL(window.location.href);
    url.searchParams.set("doc", docPath);
    return url.toString();
  }

  function resolveMarkdownLink(targetHref, currentDocPath) {
    if (!targetHref || targetHref.startsWith("http://") || targetHref.startsWith("https://") || targetHref.startsWith("#")) {
      return targetHref;
    }

    const currentDirectory = currentDocPath.includes("/")
      ? currentDocPath.substring(0, currentDocPath.lastIndexOf("/") + 1)
      : "";

    const normalizedPath = new URL(targetHref, `https://docs.local/${currentDirectory}`).pathname.replace(/^\//, "");

    if (normalizedPath.endsWith(".md")) {
      return buildDocUrl(normalizedPath);
    }

    return `../${normalizedPath}`;
  }

  function createMarkedHtml(markdown, currentDocPath) {
    const renderer = new window.marked.Renderer();

    // 將文件中的相對 Markdown 連結改成站內導覽，
    // 這樣點到其他 .md 時，使用者會留在文件網站中繼續閱讀。
    renderer.link = ({ href, title, tokens }) => {
      const text = window.marked.Parser.parseInline(tokens);
      const safeHref = resolveMarkdownLink(href, currentDocPath);
      const target = safeHref.startsWith("http") ? ' target="_blank" rel="noreferrer"' : "";
      const titleAttribute = title ? ` title="${title}"` : "";
      return `<a href="${safeHref}"${titleAttribute}${target}>${text}</a>`;
    };

    // 固定替每個標題補上 id，方便右側目錄與網址 hash 導覽。
    renderer.heading = ({ tokens, depth }) => {
      const text = window.marked.Parser.parseInline(tokens);
      return `<h${depth} id="${slugify(text)}">${text}</h${depth}>`;
    };

    return markdownRenderer.parse(markdown, { renderer, headerIds: true, mangle: false });
  }

  async function fetchDocContent(doc) {
    if (contentCache.has(doc.path)) {
      return contentCache.get(doc.path);
    }

    const response = await fetch(`../${doc.path}`);

    if (!response.ok) {
      throw new Error(`文件載入失敗: ${doc.path}`);
    }

    const markdown = await response.text();
    const normalized = markdown.replace(/\s+/g, " ").trim();
    const lower = normalized.toLowerCase();
    const payload = { markdown, normalized, lower };
    contentCache.set(doc.path, payload);
    return payload;
  }

  function buildExcerpt(normalizedText, keyword) {
    if (!keyword) {
      return "";
    }

    const lowerText = normalizedText.toLowerCase();
    const lowerKeyword = keyword.toLowerCase();
    const index = lowerText.indexOf(lowerKeyword);

    if (index < 0) {
      return "";
    }

    const start = Math.max(0, index - 48);
    const end = Math.min(normalizedText.length, index + lowerKeyword.length + 68);
    const prefix = start > 0 ? "..." : "";
    const suffix = end < normalizedText.length ? "..." : "";
    return `${prefix}${normalizedText.slice(start, end)}${suffix}`;
  }

  async function buildSearchResults(keyword) {
    const lowerKeyword = keyword.toLowerCase();
    const results = [];

    const payloads = await Promise.all(
      manifest.docs.map(async (doc) => {
        try {
          const content = await fetchDocContent(doc);
          return { doc, content };
        } catch {
          return { doc, content: null };
        }
      })
    );

    payloads.forEach(({ doc, content }) => {
      const metadataTarget = [doc.title, doc.description, doc.category, ...doc.audiences, ...doc.tags].join(" ").toLowerCase();
      const metadataMatched = metadataTarget.includes(lowerKeyword);
      const bodyMatched = Boolean(content?.lower.includes(lowerKeyword));

      if (!metadataMatched && !bodyMatched) {
        return;
      }

      const excerpt = content ? buildExcerpt(content.normalized, keyword) : doc.description;

      results.push({
        ...doc,
        searchExcerpt: excerpt || doc.description,
        matchedBy: metadataMatched && bodyMatched ? "metadata+content" : metadataMatched ? "metadata" : "content"
      });
    });

    return results;
  }

  function buildNavigation(navRoot, docs, activePath, keyword = "") {
    navRoot.innerHTML = "";

    if (docs.length === 0) {
      navRoot.innerHTML = '<div class="doc-empty">沒有找到符合條件的文件。</div>';
      return;
    }

    const groups = groupDocs(docs);

    Object.entries(groups).forEach(([groupTitle, groupDocsList]) => {
      const wrapper = document.createElement("section");
      wrapper.className = "doc-nav-group";
      wrapper.innerHTML = `<div class="doc-nav-group-title">${groupTitle}</div><div class="doc-nav-list"></div>`;
      const list = wrapper.querySelector(".doc-nav-list");

      groupDocsList.forEach((doc) => {
        const link = document.createElement("a");
        link.className = `doc-link${doc.path === activePath ? " active" : ""}`;
        link.href = buildDocUrl(doc.path);

        const summary = doc.searchExcerpt || doc.description;
        link.innerHTML = `
          <strong>${highlightText(doc.title, keyword)}</strong>
          <span>${highlightText(summary, keyword)}</span>
        `;

        list.appendChild(link);
      });

      navRoot.appendChild(wrapper);
    });
  }

  function buildToc(tocRoot, contentRoot) {
    if (!tocRoot) {
      return;
    }

    tocRoot.innerHTML = "";
    const headings = contentRoot.querySelectorAll("h2, h3");

    if (headings.length === 0) {
      tocRoot.innerHTML = '<div class="doc-empty">這份文件目前沒有可自動產生的段落目錄。</div>';
      return;
    }

    headings.forEach((heading) => {
      if (!heading.id) {
        heading.id = slugify(heading.textContent ?? "section");
      }

      const link = document.createElement("a");
      link.href = `#${heading.id}`;
      link.textContent = heading.textContent ?? "";
      link.className = heading.tagName === "H3" ? "depth-3" : "depth-2";
      tocRoot.appendChild(link);
    });
  }

  function buildMeta(metaRoot, doc) {
    if (!metaRoot) {
      return;
    }

    metaRoot.innerHTML = "";

    [doc.category, ...doc.audiences, ...doc.tags].forEach((item) => {
      const tag = document.createElement("span");
      tag.className = "doc-tag";
      tag.textContent = item;
      metaRoot.appendChild(tag);
    });
  }

  function buildPersonaCards(root) {
    if (!root || !manifest.personas) {
      return;
    }

    root.innerHTML = "";

    manifest.personas.forEach((persona) => {
      const article = document.createElement("article");
      article.className = "persona-card";
      const links = persona.docIds
        .map((docId) => getDocById(docId))
        .filter(Boolean)
        .map((doc) => `<a href="${buildDocUrl(doc.path)}">${doc.title}</a>`)
        .join("");

      article.innerHTML = `
        <h3>${persona.title}</h3>
        <p>${persona.description}</p>
        <div class="persona-links">${links}</div>
      `;

      root.appendChild(article);
    });
  }

  async function loadDocument(doc, contentRoot, titleRoot, descriptionRoot, breadcrumbRoot, metaRoot, tocRoot) {
    titleRoot.textContent = doc.title;
    descriptionRoot.textContent = doc.description;
    breadcrumbRoot.textContent = `${doc.category} / ${doc.title}`;
    buildMeta(metaRoot, doc);
    contentRoot.innerHTML = '<div class="doc-empty">文件載入中，請稍候...</div>';

    try {
      const content = await fetchDocContent(doc);
      const rendered = createMarkedHtml(content.markdown, doc.path);
      const safeHtml = window.DOMPurify ? window.DOMPurify.sanitize(rendered) : rendered;
      contentRoot.innerHTML = safeHtml;
      buildToc(tocRoot, contentRoot);
      localStorage.setItem("enterprise-docs-last-doc", doc.path);
    } catch (error) {
      contentRoot.innerHTML = `
        <div class="doc-empty">
          這份文件目前載入失敗。<br /><br />
          詳細訊息：${error.message}
        </div>
      `;
    }
  }

  function setSearchStatus(statusRoot, message) {
    if (statusRoot) {
      statusRoot.textContent = message;
    }
  }

  function bootstrap() {
    const navRoot = document.querySelector("[data-doc-nav]");
    const contentRoot = document.querySelector("[data-doc-content]");
    const titleRoot = document.querySelector("[data-doc-title]");
    const descriptionRoot = document.querySelector("[data-doc-description]");
    const breadcrumbRoot = document.querySelector("[data-doc-breadcrumb]");
    const metaRoot = document.querySelector("[data-doc-meta]");
    const tocRoot = document.querySelector("[data-doc-toc]");
    const searchInput = document.querySelector("[data-doc-search]");
    const searchStatusRoot = document.querySelector("[data-doc-search-status]");
    const personaRoot = document.querySelector("[data-persona-grid]");

    if (!navRoot || !contentRoot || !titleRoot || !descriptionRoot || !breadcrumbRoot) {
      return;
    }

    const savedDoc = localStorage.getItem("enterprise-docs-last-doc");
    const queryDoc = new URL(window.location.href).searchParams.get("doc");
    const initialPath = queryDoc || savedDoc || manifest.defaultDoc;

    let filteredDocs = [...manifest.docs];
    let activeDoc = getDocByPath(initialPath);
    let searchVersion = 0;

    buildNavigation(navRoot, filteredDocs, activeDoc.path);
    buildPersonaCards(personaRoot);
    loadDocument(activeDoc, contentRoot, titleRoot, descriptionRoot, breadcrumbRoot, metaRoot, tocRoot);

    searchInput?.addEventListener("input", async () => {
      const keyword = searchInput.value.trim();
      const currentSearchVersion = ++searchVersion;

      if (!keyword) {
        filteredDocs = [...manifest.docs];
        buildNavigation(navRoot, filteredDocs, activeDoc.path);
        setSearchStatus(searchStatusRoot, "");
        return;
      }

      setSearchStatus(searchStatusRoot, "全文搜尋中，第一次會稍慢一點...");

      const results = await buildSearchResults(keyword);

      if (currentSearchVersion !== searchVersion) {
        return;
      }

      filteredDocs = results;

      if (!filteredDocs.some((doc) => doc.path === activeDoc.path)) {
        activeDoc = filteredDocs[0] ?? getDocByPath(manifest.defaultDoc);
      }

      buildNavigation(navRoot, filteredDocs, activeDoc.path, keyword);
      setSearchStatus(searchStatusRoot, `找到 ${filteredDocs.length} 份相關文件。`);
    });

    document.addEventListener("click", async (event) => {
      const target = event.target instanceof HTMLElement ? event.target.closest("a") : null;

      if (!target) {
        return;
      }

      const href = target.getAttribute("href");

      if (!href) {
        return;
      }

      const url = new URL(href, window.location.href);
      const docPath = url.searchParams.get("doc");

      if (!docPath) {
        return;
      }

      event.preventDefault();
      activeDoc = getDocByPath(docPath);
      buildNavigation(navRoot, filteredDocs, activeDoc.path, searchInput?.value.trim() ?? "");
      history.pushState({}, "", buildDocUrl(activeDoc.path));
      await loadDocument(activeDoc, contentRoot, titleRoot, descriptionRoot, breadcrumbRoot, metaRoot, tocRoot);
    });

    window.addEventListener("popstate", async () => {
      const docPath = new URL(window.location.href).searchParams.get("doc") || manifest.defaultDoc;
      activeDoc = getDocByPath(docPath);
      buildNavigation(navRoot, filteredDocs, activeDoc.path, searchInput?.value.trim() ?? "");
      await loadDocument(activeDoc, contentRoot, titleRoot, descriptionRoot, breadcrumbRoot, metaRoot, tocRoot);
    });
  }

  document.addEventListener("DOMContentLoaded", bootstrap);
})();
