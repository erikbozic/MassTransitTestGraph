using MassTransit.SagaStateMachine;
using MassTransit.Visualizer;
using MassTransitTestGraph;

var machine = new MyStateMachine();

var graph = machine.GetGraph();

StateMachineGraphvizGenerator generator = new(graph);

Console.WriteLine(generator.CreateDotFile());


