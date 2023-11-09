namespace Godot.EGP;

public interface IEventFilter
{
	bool Match(IEvent matchEvent);
}
