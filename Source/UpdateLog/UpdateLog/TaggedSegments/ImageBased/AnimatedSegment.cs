using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace UpdateLogTool
{
	public abstract class AnimatedSegment : ContainerSegment
	{
		protected abstract int DefaultFramesPerSecond { get; }

		protected virtual int DefaultDelayOnReset { get; } = 0;

		protected virtual int CurrentFrame(int maxFrames, int fps, int delay)
		{
			return Mathf.Clamp((Mathf.FloorToInt(Time.time * fps) % (maxFrames + delay)) - delay, 0, maxFrames);
		}

		protected override Dictionary<string, object> HandleCustomAttribute(string name, string value)
		{
			Dictionary<string, object> lookup = new Dictionary<string, object>();
			int fps = DefaultFramesPerSecond;
			int delay = 0;
			if (name.ToUpperInvariant() == "FPS")
			{
				int.TryParse(value, out fps);
			}
			else if (name.ToUpperInvariant() == "DELAY")
			{
				int.TryParse(value, out delay);
			}
			else
			{
				base.HandleCustomAttribute(name, value);
			}
			lookup.Add("FPS", fps);
			lookup.Add("DELAY", delay);
			return lookup;
		}
	}
}
