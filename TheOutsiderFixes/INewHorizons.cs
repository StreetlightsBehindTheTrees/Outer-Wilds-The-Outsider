using OWML.Common;

namespace TheOutsiderFixes
{
    public interface INewHorizons
    {
        /// <summary>
        /// The name of the current star system loaded.
        /// </summary>
        string GetCurrentStarSystem();

        /// <summary>
        /// Registers a subtitle for the main menu.
        /// Call this once before the main menu finishes loading
        /// </summary>
        /// <param name="mod"></param>
        /// <param name="filePath"></param>
        void AddSubtitle(IModBehaviour mod, string filePath);
    }
}
