using System;

public class QuickListener{

	public const int QUICK_TYPE_ALL = 0;

	/// <summary>
	/// Zero means the listener will hear about all events.
	/// Can be set to listen for a single event code, or any combination of event codes simply by = type1 + type2 + type5;
	/// </summary>
	public int registeredTypes = QUICK_TYPE_ALL;

	public QuickCallback callback;

	public delegate void QuickCallback(int eventCode, System.Object data);

	public QuickListener(QuickCallback callback ) {
		this.callback = callback;
	}

	public QuickListener(QuickCallback callback, int types) {
		this.callback = callback;
		registeredTypes = types;
	}

	/// <summary>
	/// Hacky convenience method that takes any number of enums and creates a listener that only hears about those types of events
	/// </summary>
	/// <param name="callback"></param>
	/// <param name="types"></param>
	public QuickListener(QuickCallback callback, params object[] types) {
		int type = 0;
		foreach (var t in types) {
			type += t.GetHashCode();
		}
		this.callback = callback;
		registeredTypes = type;
	}

}

