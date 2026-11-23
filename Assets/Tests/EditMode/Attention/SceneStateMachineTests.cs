using System.Collections.Generic;
using NUnit.Framework;

/// <summary>
/// The state machine walks the scene through named phases. It starts on the first state, moves
/// to a state when its OnEnter event is raised, and raises StateChanged so other components
/// (like the AttentionDirector) can react.
/// </summary>
public class SceneStateMachineTests : EditModeTest
{
    private SceneStateMachine machine;
    private GameEvent bottlePickedUp;
    private GameEvent bottleDelivered;
    private SceneState opening;
    private SceneState carrying;
    private SceneState resting;

    [SetUp]
    public void SetUp()
    {
        bottlePickedUp = Track(TestData.Event("pickedUp"));
        bottleDelivered = Track(TestData.Event("delivered"));

        opening = new SceneState("Opening", null, bottlePickedUp);
        carrying = new SceneState("Carrying", bottlePickedUp, bottleDelivered);
        resting = new SceneState("Resting", bottleDelivered, null);

        machine = Make("Machine").AddComponent<SceneStateMachine>();
        TestUtils.Set(machine, "states", new List<SceneState> { opening, carrying, resting });
        TestUtils.Call(machine, "OnEnable");
    }

    [Test]
    public void AtStart_TheMachineIsInTheFirstState()
    {
        Assert.AreSame(opening, machine.Current);
        Assert.AreEqual(0, machine.CurrentIndex);
    }

    [Test]
    public void ARaisedOnEnter_MovesTheMachineToThatState()
    {
        bottlePickedUp.Raise();

        Assert.AreSame(carrying, machine.Current);
        Assert.AreEqual(1, machine.CurrentIndex);
    }

    [Test]
    public void TwoTransitions_WalkThroughTheStates()
    {
        bottlePickedUp.Raise();
        bottleDelivered.Raise();

        Assert.AreSame(resting, machine.Current);
        Assert.AreEqual(2, machine.CurrentIndex);
    }

    [Test]
    public void StateChanged_FiresForEveryTransition()
    {
        var seen = new List<string>();
        machine.StateChanged += state => seen.Add(state.Name);

        bottlePickedUp.Raise();
        bottleDelivered.Raise();

        CollectionAssert.AreEqual(new[] { "Carrying", "Resting" }, seen);
    }

    [Test]
    public void GoTo_JumpsStraightToTheGivenIndex()
    {
        machine.GoTo(2);

        Assert.AreSame(resting, machine.Current);
    }

    [Test]
    public void GoTo_OutOfRange_IsIgnored()
    {
        machine.GoTo(-1);
        machine.GoTo(999);

        Assert.AreSame(opening, machine.Current);
    }

    [Test]
    public void OnDisable_StopsListeningToTheEvents()
    {
        TestUtils.Call(machine, "OnDisable");

        bottlePickedUp.Raise();

        Assert.AreSame(opening, machine.Current);
    }
}
