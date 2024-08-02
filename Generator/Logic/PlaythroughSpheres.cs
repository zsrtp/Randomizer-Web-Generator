namespace TPRandomizer
{
    using System.Collections.Generic;

    public class PlaythroughSpheres
    {
        public HashSet<string> sphere0Checks { get; } = null;
        public List<List<KeyValuePair<int, Item>>> spheresVerbose { get; } = null;
        public List<List<KeyValuePair<int, Item>>> spheres { get; } = null;

        public PlaythroughSpheres(
            HashSet<string> sphere0Checks,
            List<List<KeyValuePair<int, Item>>> spheresVerbose,
            List<List<KeyValuePair<int, Item>>> spheres
        )
        {
            if (sphere0Checks == null)
                this.sphere0Checks = new();
            else
                this.sphere0Checks = sphere0Checks;
            this.spheresVerbose = spheresVerbose;
            this.spheres = spheres;
        }
    }
}
