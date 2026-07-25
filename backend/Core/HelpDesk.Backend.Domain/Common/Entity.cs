namespace HelpDesk.Backend.Domain.Common;

public abstract class Entity
{
    protected Entity()
    {
    }

    protected Entity(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("ENTITY_ID_REQUIRED", "La identidad de la entidad es obligatoria.");
        }

        Id = id;
    }

    public Guid Id { get; private set; }
}
