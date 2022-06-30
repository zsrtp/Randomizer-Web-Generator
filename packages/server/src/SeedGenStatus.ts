enum SeedGenProgress {
  Queued = 'Queued',
  Started = 'Started',
  // Add other ones here as reported from the C# output while it is being worked
  // on?
  Done = 'Done',
  Error = 'Error',
  // Abandoned are ones which appear to be abandoned. This means no one is
  // checking on its status, so we won't actually run the generation unless
  // someone starts checking on it again. Eventually, it will be completely
  // deleted if no one checks on it.
  Abandoned = 'Abandoned',
}

class SeedGenStatus {
  readonly seedId: string;
  readonly settingsString: string;
  readonly seed: string;
  // requesterHash is passed to the client-side to prevent unnecessary progress
  // check calls from people who did not request the generation. The page will
  // display as if the id is invalid until the generation successfully
  // completes.
  readonly requesterHash: string;
  readonly userId: string;

  private _lastRefreshed: number;
  private _progress: SeedGenProgress;

  constructor(
    seedId: string,
    userId: string,
    requesterHash: string,
    settingsString: string,
    seed: string
  ) {
    this.seedId = seedId;
    this.userId = userId;
    this._lastRefreshed = new Date().getTime();
    this._progress = SeedGenProgress.Queued;
    this.requesterHash = requesterHash;
    this.settingsString = settingsString;
    this.seed = seed;
  }

  get lastRefreshed() {
    return this._lastRefreshed;
  }

  updateRefreshTime(): void {
    this._lastRefreshed = new Date().getTime();
  }

  get progress() {
    return this._progress;
  }

  set progress(progress: SeedGenProgress) {
    this._progress = progress;
    if (progress !== SeedGenProgress.Abandoned) {
      this._lastRefreshed = new Date().getTime();
    }
  }

  isHanging() {
    return (
      this._progress === SeedGenProgress.Abandoned ||
      this._progress === SeedGenProgress.Error
    );
  }
}

export default SeedGenStatus;

export { SeedGenProgress };
