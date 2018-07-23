using System.Collections.Generic;

public class Parallel : ITask {

	private List<ITask> tasks = new List<ITask>();

	private Dictionary<ITask,bool> taskResults = new Dictionary<ITask, bool>();
	

	public bool LoopEachOnFinish { get; private set; }

	public Parallel( bool loopEachOnFinish = true ) {
		LoopEachOnFinish = loopEachOnFinish;
	}
	
	public void Add(ITask task ) {
		tasks.Add(task);
		taskResults.Add(task,false);
	}

	public bool Update(Blackboard blackboard) {
		foreach (var task in tasks) {
			if (LoopEachOnFinish || !taskResults[task]) {
				taskResults[task] = task.Update(blackboard);
			}
		}
		return false; // TODO: return true if all tasks are finished and there is no looping
	}
}
