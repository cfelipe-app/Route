using Microsoft.EntityFrameworkCore;
using Route.Backend.Repositories.Interfaces;
using Route.Backend.UnitsOfWork.Interfaces;
using Route.Shared.Entities;
using Route.Shared.Enums;

namespace Route.Backend.Application.Offers
{
    public class OffersService : IOffersService
    {
        private readonly IGenericRepository<VehicleOffer> _offerRepo;
        private readonly IGenericRepository<CapacityRequest> _capReqRepo;
        private readonly IGenericUnitOfWork<VehicleOffer> _offerUow;   // para persistir cambios en oferta
        private readonly IGenericUnitOfWork<CapacityRequest> _capReqUow;// para persistir cambios en request

        public OffersService(
            IGenericRepository<VehicleOffer> offerRepo,
            IGenericRepository<CapacityRequest> capReqRepo,
            IGenericUnitOfWork<VehicleOffer> offerUow,
            IGenericUnitOfWork<CapacityRequest> capReqUow)
        {
            _offerRepo = offerRepo;
            _capReqRepo = capReqRepo;
            _offerUow = offerUow;
            _capReqUow = capReqUow;
        }

        public async Task AcceptAsync(int offerId, string decidedBy, CancellationToken ct = default)
        {
            // 1) Cargar oferta + CR (usa Query para incluir la navegación)
            var offer = await _offerRepo.Query()
                .Include(o => o.CapacityRequest)
                .FirstOrDefaultAsync(o => o.Id == offerId, ct);

            if (offer is null)
                throw new InvalidOperationException("Offer not found.");

            if (offer.Status is not VehicleOfferStatus.Sent and not VehicleOfferStatus.Draft)
                throw new InvalidOperationException("La oferta no puede ser aceptada en su estado actual.");

            // 2) Actualizar oferta
            offer.Status = VehicleOfferStatus.Accepted;
            offer.DecisionAt = DateTime.UtcNow;
            offer.DecidedBy = decidedBy;

            var updOffer = await _offerUow.UpdateAsync(offer);
            if (!updOffer.WasSuccess)
                throw new InvalidOperationException(updOffer.Message ?? "No se pudo actualizar la oferta.");

            // 3) Recalcular cobertura y actualizar CR
            var req = offer.CapacityRequest;

            var covered = await _offerRepo.Query()
                .Where(o => o.CapacityRequestId == req.Id && o.Status == VehicleOfferStatus.Accepted)
                .Select(o => o.OfferedWeightKg * (o.Quantity <= 0 ? 1 : o.Quantity))
                .SumAsync(ct);

            if (covered <= 0)
                req.Status = CapacityReqStatus.Quoted;
            else if (covered < req.DemandWeightKg)
                req.Status = CapacityReqStatus.PartiallyAwarded;
            else
                req.Status = CapacityReqStatus.Closed;

            var updReq = await _capReqUow.UpdateAsync(req);
            if (!updReq.WasSuccess)
                throw new InvalidOperationException(updReq.Message ?? "No se pudo actualizar el requerimiento.");
        }

        public async Task RejectAsync(int offerId, string decidedBy, CancellationToken ct = default)
        {
            var getResponse = await _offerUow.GetAsync(offerId); // o _offerRepo.GetAsync(offerId)
            if (!getResponse.WasSuccess || getResponse.Result is null)
                throw new InvalidOperationException("Offer not found.");

            var offer = getResponse.Result;

            if (offer.Status is VehicleOfferStatus.Accepted or VehicleOfferStatus.Rejected)
                throw new InvalidOperationException("La oferta ya fue decidida.");

            offer.Status = VehicleOfferStatus.Rejected;
            offer.DecisionAt = DateTime.UtcNow;
            offer.DecidedBy = decidedBy;

            var updOffer = await _offerUow.UpdateAsync(offer);
            if (!updOffer.WasSuccess)
                throw new InvalidOperationException(updOffer.Message ?? "No se pudo actualizar la oferta.");
        }
    }
}