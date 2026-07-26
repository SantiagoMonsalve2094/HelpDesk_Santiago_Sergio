using FluentValidation;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Application.DTOs.Sla;
using HelpDesk.Backend.Application.DTOs.Tickets;
using HelpDesk.Backend.Application.Features.Tickets;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Queries.GetTicketById;

public sealed class GetTicketByIdHandler(
    IUnitOfWork unitOfWork,
    IValidator<GetTicketByIdQuery> validator)
    : IRequestHandler<GetTicketByIdQuery, TicketDetailsResponse>
{
    public async Task<TicketDetailsResponse> Handle(
        GetTicketByIdQuery request,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var actor = await ApplicationAccess.GetUserAsync(
            unitOfWork,
            request.ActorUserId,
            cancellationToken);
        var ticket = await ApplicationAccess.GetTicketAsync(
            unitOfWork,
            request.TicketId,
            cancellationToken);
        TicketApplicationAccess.EnsureCanView(actor, ticket);
<<<<<<< HEAD
        var authorIds = ticket.Comments
            .Select(comment => comment.AuthorUserId)
            .Distinct()
            .ToArray();
        var authorNames = new Dictionary<Guid, string>(authorIds.Length);
        foreach (var authorId in authorIds)
        {
            var author = await unitOfWork.Users.GetByIdAsync(authorId, cancellationToken);
            if (author is not null)
            {
                authorNames[authorId] = author.FullName;
            }
        }

        return TicketMapper.ToDetails(ticket, authorNames);
=======
        return TicketMapper.ToDetails(ticket);
>>>>>>> 60bd3aa8c163527f2e018e15a29114b99aa06847
    }
}
