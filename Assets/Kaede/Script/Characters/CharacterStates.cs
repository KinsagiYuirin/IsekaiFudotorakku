using MadDuck.Scripts.Frameworks;
using MadDuck.Scripts.Frameworks.MessagePipe;

namespace MadDuck.Scripts.Characters
{
    public enum CharacterMovementState
    {
        Idle,
        Walking
    }

    public struct MovementStateEvent : ICharacterHubIdentifier
    {
        public CharacterHub IdentifierObject { get; }
        public CharacterMovementState previousState;
        public CharacterMovementState newState;

        public MovementStateEvent(CharacterHub obj, CharacterMovementState previousState, CharacterMovementState newState)
        {
            IdentifierObject = obj;
            this.previousState = previousState;
            this.newState = newState;
        }
    }

    public enum CharacterActionState
    {
        None,
        Fishing,
        Sailing
    }

    public struct ActionStateEvent : ICharacterHubIdentifier
    {
        public CharacterHub IdentifierObject { get; }
        public CharacterActionState previousState;
        public CharacterActionState newState;

        public ActionStateEvent(CharacterHub obj, CharacterActionState previousState, CharacterActionState newState)
        {
            IdentifierObject = obj;
            this.previousState = previousState;
            this.newState = newState;
        }
    }

    public enum CharacterConditionState
    {
        Normal,
        Dead,
        Carrying
    }

    public struct ConditionStateEvent : ICharacterHubIdentifier
    {
        public CharacterHub IdentifierObject { get; }
        public CharacterConditionState previousState;
        public CharacterConditionState newState;

        public ConditionStateEvent(CharacterHub characterHub, CharacterConditionState previousState,
            CharacterConditionState newState)
        {
            IdentifierObject = characterHub;
            this.previousState = previousState;
            this.newState = newState;
        }
    }
}