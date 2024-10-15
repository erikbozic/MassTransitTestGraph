using MassTransit;

namespace MassTransitTestGraph;

public class MyStateMachine : MassTransitStateMachine<MyThingState>
{
    public State Created { get; set; }
    public State Registered { get; set; }
    public State Rejected { get; set; }
    
    public Event<CreatedEvent> ThingCreated { get; set; } = default!;
    public Event<RegisteredEvent> ThingRegistered { get; set; } = default!;
    public Event<ApprovedEvent> ThingApproved { get; set; } = default!;
    public Event<DisapprovedEvent> ThingDisapproved { get; set; } = default!;
    public Event<ValidatedEvent> ThingValidated { get; set; } = default!;
    public Event<ValidationFailedEvent> ThingValidationFailed { get; set; } = default!;

    public MyStateMachine()
    {
        InstanceState(myThing => myThing.CurrentState);
        
        Event(() => ThingCreated, configurator => configurator.CorrelateById(context => context.Message.ThingId));
        Event(() => ThingRegistered, configurator => configurator.CorrelateById(context => context.Message.ThingId));
        Event(() => ThingApproved, configurator => configurator.CorrelateById(context => context.Message.ThingId));
        Event(() => ThingDisapproved, configurator => configurator.CorrelateById(context => context.Message.ThingId));
        Event(() => ThingValidated, configurator => configurator.CorrelateById(context => context.Message.ThingId));
        
        Initially(
            When(ThingCreated)
                .TransitionTo(Created));
        
        During(Created,
            When(ThingRegistered)
                .TransitionTo(Registered));

        // according to the docs this has to be declared after all other events and behaviors are declared
        // (https://masstransit.io/documentation/patterns/saga/state-machine#composite-event)
        CompositeEvent(() => ThingConfirmed, x => x.CompositeStatus, ThingApproved, ThingValidated);

        During(Registered,
            When(ThingConfirmed)
                .Finalize(),
             When(ThingDisapproved)
                 .TransitionTo(Rejected),
            When(ThingValidationFailed)
                .TransitionTo(Rejected));
        
    }
    public Event ThingConfirmed { get; set; } = default!;

}

public class ValidationFailedEvent : ThingEvent;

public class DisapprovedEvent : ThingEvent;

public class ValidatedEvent : ThingEvent;

public class ApprovedEvent : ThingEvent;

public class RegisteredEvent : ThingEvent;

public class CreatedEvent: ThingEvent;

public abstract class ThingEvent
{
    public Guid ThingId { get; set; }
}

public class MyThingState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public State CurrentState { get; set; }
    
    /// <summary>
    /// Used for combined events regarding ThingApproved and ThingValidated
    /// </summary>
    public int CompositeStatus { get; set; }
}
