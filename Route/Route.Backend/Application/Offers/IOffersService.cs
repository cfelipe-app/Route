namespace Route.Backend.Application.Offers
{
    public interface IOffersService
    {
        Task AcceptAsync(int offerId, string decidedBy, CancellationToken ct = default);

        Task RejectAsync(int offerId, string decidedBy, CancellationToken ct = default);
    }
}