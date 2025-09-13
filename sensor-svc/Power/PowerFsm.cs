namespace SensorSvc.Power;

public enum FsmState
{
	Active,
	Idle,
	Sleep
}

public sealed class PowerFsm
{
	private readonly CancellationTokenSource _cts = new();
	private readonly object _lock = new();
	public FsmState CurrentState { get; private set; } = FsmState.Active;
	
	// Time-in-state counters (seconds)
	private double _sAct = 0, _sIdle = 0, _sSlp = 0;
	private DateTime _prevTime = DateTime.UtcNow;

	private readonly int _sampleActiveHz;
	private readonly int _sampleIdleHz;
	private readonly int _activeWindowS;
	private readonly int _sleepAfterS;
	private readonly bool _enabled;

	public PowerFsm()
	{
		_enabled = ReadBoolEnv("POWER_FSM", defaultValue: true); // Enable FSM by default

		_sampleActiveHz = ReadIntEnv("SAMPLE_ACTIVE_HZ", 100);
		_sampleIdleHz = ReadIntEnv("SAMPLE_IDLE_HZ", 1);
		_activeWindowS = ReadIntEnv("ACTIVE_WINDOW_S", 5); // Shorter window to go to Idle faster
		_sleepAfterS = ReadIntEnv("SLEEP_AFTER_S", 30); // Shorter sleep delay
	}

	public void Start(Func<long> getLastRequestMs)
	{
		_ = Task.Run(async () =>
		{
			if (!_enabled)
			{
				lock (_lock) CurrentState = FsmState.Active;
			}

			while (!_cts.IsCancellationRequested)
			{
				try
				{
					UpdateStateIfNeeded(getLastRequestMs);

					int delayMs = CurrentState switch
					{
						FsmState.Active => Math.Max(1, 1000 / Math.Max(1, _sampleActiveHz)),
						FsmState.Idle => Math.Max(1, 1000 / Math.Max(1, _sampleIdleHz)),
						FsmState.Sleep => 1000,
						_ => 1000
					};

					await Task.Delay(delayMs, _cts.Token);
				}
				catch (TaskCanceledException) { }
				catch { }
			}
		});
	}

	private void UpdateStateIfNeeded(Func<long> getLastRequestMs)
	{
		if (!_enabled) return;

		var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		var lastReqMs = getLastRequestMs();
		var sinceReqS = (nowMs - lastReqMs) / 1000.0;

		var newState = CurrentState;
		switch (CurrentState)
		{
			case FsmState.Active:
				if (sinceReqS >= _activeWindowS) newState = FsmState.Idle;
				break;
			case FsmState.Idle:
				if (sinceReqS >= _activeWindowS + _sleepAfterS) newState = FsmState.Sleep;
				break;
			case FsmState.Sleep:
				break;
		}

		if (sinceReqS < _activeWindowS)
		{
			newState = FsmState.Active;
		}

		if (newState != CurrentState)
		{
			lock (_lock) CurrentState = newState;
		}
		
		// Accumulate time in current state
		var now = DateTime.UtcNow;
		var dt = (now - _prevTime).TotalSeconds;
		_prevTime = now;
		switch (CurrentState)
		{
			case FsmState.Active: _sAct += dt; break;
			case FsmState.Idle: _sIdle += dt; break;
			case FsmState.Sleep: _sSlp += dt; break;
		}
	}

	public void Stop() => _cts.Cancel();
	
	// Get time-in-state counters
	public (double sAct, double sIdle, double sSlp) GetTimeInState()
	{
		lock (_lock) return (_sAct, _sIdle, _sSlp);
	}
	
	// Reset time counters
	public void ResetTimeCounters()
	{
		lock (_lock) { _sAct = _sIdle = _sSlp = 0; _prevTime = DateTime.UtcNow; }
	}

	static bool ReadBoolEnv(string name, bool defaultValue)
	{
		var v = Environment.GetEnvironmentVariable(name);
		if (string.IsNullOrWhiteSpace(v)) return defaultValue;
		return v.Trim() switch
		{
			"1" => true,
			"true" => true,
			"True" => true,
			"TRUE" => true,
			_ => false
		};
	}

	static int ReadIntEnv(string name, int defaultValue)
	{
		var v = Environment.GetEnvironmentVariable(name);
		if (string.IsNullOrWhiteSpace(v)) return defaultValue;
		return int.TryParse(v, out var i) ? i : defaultValue;
	}
}


