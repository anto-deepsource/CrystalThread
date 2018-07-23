using System;

public class QuickListener{

	/// <summary>
	/// Zero means the listener will hear about all events.
	/// Can be set to listen for a single event code, or any combination of event codes simply by = type1 + type2 + type5;
	/// </summary>
	public int registeredTypes = 0;

	public QuickCallback callback;

	public delegate void QuickCallback(int eventCode, System.Object data);

	public QuickListener(QuickCallback callback ) {
		this.callback = callback;
	}

	public QuickListener(QuickCallback callback, int types) {
		this.callback = callback;
		registeredTypes = types;
	}
}
