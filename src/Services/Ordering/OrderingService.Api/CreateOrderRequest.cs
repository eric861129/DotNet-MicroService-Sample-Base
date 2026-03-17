using System.Net.Mail;

namespace OrderingService.Api;

public sealed record CreateOrderRequest(string CustomerEmail, IReadOnlyCollection<CreateOrderItemRequest> Items)
{
    public Dictionary<string, string[]> Validate()
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(CustomerEmail))
        {
            errors["customerEmail"] = ["Customer email is required."];
        }
        else
        {
            try
            {
                _ = new MailAddress(CustomerEmail);
            }
            catch (FormatException)
            {
                errors["customerEmail"] = ["Customer email must be a valid email address."];
            }
        }

        if (Items.Count == 0)
        {
            errors["items"] = ["At least one order item is required."];
        }

        if (Items.Any(item => item.ProductId == Guid.Empty || item.Quantity <= 0))
        {
            errors["items"] = ["Each order item must include a productId and quantity greater than zero."];
        }

        return errors;
    }
}

public sealed record CreateOrderItemRequest(Guid ProductId, int Quantity);
