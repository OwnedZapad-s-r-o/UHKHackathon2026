using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Api.Services;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain;

namespace SmartAudit.Api.Features.Status.GetStatus
{
    public record GetStatusCommand : IRequest<Response<GetStatusDto>>;

    public record GetStatusDto(string Status, string Phase, bool Ready, int Facilities, int Sensors, DateTimeOffset? LastReadingAt);

    public class GetStatusHandler : IRequestHandler<GetStatusCommand, Response<GetStatusDto>>
    {
        private readonly SmartAuditDbContext _dbContext;
        private readonly IPipelineStateService _state;

        public GetStatusHandler(SmartAuditDbContext dbContext, IPipelineStateService state)
        {
            _dbContext = dbContext;
            _state = state;
        }

        public async Task<Response<GetStatusDto>> Handle(GetStatusCommand request, CancellationToken ct)
        {
            // Během migrace tabulky ještě nemusí existovat
            if (!_state.IsReady)
                return Response.Success(new GetStatusDto("starting", _state.Phase, false, 0, 0, null));

            var facilities = await _dbContext.Facilities.CountAsync(ct);
            var sensors = await _dbContext.Sensors.CountAsync(ct);
            var lastReadingAt = await _dbContext.FacilityAssessments.MaxAsync(a => (DateTimeOffset?)a.EvaluatedAt, ct);

            return Response.Success(new GetStatusDto("ok", _state.Phase, true, facilities, sensors, lastReadingAt));
        }
    }
}
