# GitHub Copilot Instructions for Smarter Deconstruction and Mining (Continued) Mod Project

## Mod Overview and Purpose
**Mod Name:** Smarter Deconstruction and Mining (Continued)  
**Author:** legodude17 (Continued by GGVikky on the RimWorld Mod Market)  
**PackageId:** Mlie.SmarterDeconstructionAndMining  

The Smarter Deconstruction and Mining mod enhances the deconstruction and mining processes in RimWorld by ensuring that pawns do not inadvertently cause roof collapses, thereby protecting them from potential harm. This mod is particularly useful for efficiently dismantling buildings, removing random structures, and clearing mountains without the constant worry of roof-related accidents.

## Key Features and Systems
- **Roof Safety Checks:** Options to toggle roof checks during deconstruction and mining, ensuring pawns do not cause collapses.
- **Designation Management:** Automatically cancel designations that would lead to collapses due to mountain roofs, unless manually overridden.
- **Compatibility Recognition:** Automatically detects and integrates with mods like Expanded Roofing and Raise the Roof, adjusting relevant settings automatically.
- **Research Integration:** Requires research before removing mountain roofs, configurable with integrated mods.

## Coding Patterns and Conventions
- **Organized Namespace and Classes:** Use clear namespace and class definitions to categorize functionalities (e.g., JobInfo, Main, SmartDeconSettings).
- **Modular Structure:** Break down each module into separate files to maintain a clean and manageable codebase. Consider the S.O.L.I.D principles when adding/modifying features.
- **Consistent Naming Conventions:** Follow CamelCase for methods and PascalCase for classes and public members to maintain consistency throughout the C# codebase.
- **Descriptive Function Naming:** Ensure that method names like `CheckForRoofsBeforeJob()` are descriptive of their purpose, aiding in readability.

## XML Integration
- **Configuration Management:** The `About.xml` file is used for defining mod metadata. It should include clear declarations of dependencies, namely the brrainz.harmony library.
- **Integration Hooks:** Provide hooks or entries in the XML for easy configuration of mod settings if applicable.

## Harmony Patching
- **Library Use:** The mod employs the Harmony library for patching existing RimWorld methods to integrate custom roof-checking logic without altering core game files.
- **Safe Method Overwrite:** Utilize Harmony patches for methods related to job designations to safely insert custom functionality. Ensure patches are conditional and reversible to maintain compatibility with other mods.
- **Performance Considerations:** Limit the impact of Harmony patches on performance by carefully selecting only necessary methods to patch.

## Suggestions for Copilot
- **Contextual Code Suggestions:** When writing new code, guide Copilot to suggest methods that integrate extensively with existing logic, especially in `SmartDeconstructMod`.
- **Pattern Recognition:** Encourage Copilot to detect and follow the mod’s existing code style and structure during autocompletions.
- **Common Functionality:** Utilize Copilot to autocomplete frequently used patterns such as error handling and user feedback messages.
- **Automated Testing:** Suggest boilerplate test cases for newly added features to ensure robustness.

By adhering to these guidelines, Copilot can be effectively harnessed to maintain and enhance the Smarter Deconstruction and Mining mod for RimWorld, ensuring that it remains a valuable tool for players who seek smarter mining and deconstruction techniques.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).

