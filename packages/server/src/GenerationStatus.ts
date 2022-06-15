class GenerationStatus {
  timestamp: number;
  done: boolean;
  progress: string;
  error: boolean;
  settingsString: string;
  seed: string;

  constructor(settingsString: string, seed: string) {
    this.timestamp = new Date().getTime();
    this.done = false;
    this.progress = 'queued';
    this.error = false;
    this.settingsString = settingsString;
    this.seed = seed;
  }

  markDone() {
    this.timestamp = new Date().getTime();
    this.done = true;
  }

  updateProgress(progress: string) {
    this.timestamp = new Date().getTime();
    this.progress = progress;
  }

  markError() {
    this.timestamp = new Date().getTime();
    this.error = true;
  }
}

export default GenerationStatus;
