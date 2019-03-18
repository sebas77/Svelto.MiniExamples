using System;

namespace Svelto.ECS
{
    struct EntitySubmitOperation
    {
        public readonly EntitySubmitOperationType type;
        public readonly IEntityBuilder[]          builders;
        public readonly EGID                      fromID;
        public readonly EGID                      toID;
        public readonly Type                      entityDescriptor;
#if DEBUG && !PROFILER
        public string trace;
#endif

        public EntitySubmitOperation(EntitySubmitOperationType operation, EGID from, EGID to,
                                     IEntityBuilder[]          builders         = null,
                                     Type                      entityDescriptor = null)
        {
            type          = operation;
            this.builders = builders;
            fromID            = from;
            toID          = to;

            this.entityDescriptor = entityDescriptor;
#if DEBUG && !PROFILER
            trace = string.Empty;
#endif
        }
    }

    enum EntitySubmitOperationType
    {
        Swap,
        Remove,
        RemoveGroup
    }
}