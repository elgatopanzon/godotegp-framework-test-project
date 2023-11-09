namespace GodotEGP.Event.Filter;

using GodotEGP.Event.Events;

public interface IFilter
{
	bool Match(IEvent matchEvent);
}
