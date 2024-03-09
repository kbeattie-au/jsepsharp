# Contributing Guide

## Guidelines

No linter is set up yet, but there are some basic guidelines that should be followed. I plan to enforce some of the style conventions via a linter in the future.

### Coding Style and Conventions

* Avoid excess whitespace between lines.
* Use 4 spaces for indention.
* Start braces on a new line.
* Avoid nested ternaries.
* Place spaces after commas, not before.
* `if` statements should have braces.
* Prefer use of guard `if` over `if`/`else` nesting.
* Unreachable code should be avoided.
* Code removal over commented-out code is preferred.
* Implement suggested changes from code analysis, or use suppression attributes with a justification.

### Commit Messages

* If an issue exists, prefix the commits with the issue number.
* Use standard capitalization. Don't use all UPPERCASE or lowercase in your commit messages.
* Specify what is contained in the commit (bug fix, refactor, new feature).
* What and why are important:
  * *"Fixed some problems"* is not a good commit message, because it doesn't describe what problems were actually addressed.
  * *"Changed `JELLY_BEAN` constant to 5"* isn't ideal either, because it duplicates the code changes and doesn't explain why it was necessary to change the constant in the first place.
  * *"Changes based on PR feedback"* doesn't describe the what or why, without a participant having to go through the PR history to try and figure out what that refers to. It is fine to mention a change is the result of feedback.

### Pull Requests

* No compiler warnings should be present.
* No test failures should be present.
* New tests should be written for new functionality.
* Use the `security` tag, if your change contains security fixes.
* Use the `bugfix` tag, if your changes contain a bug fix.
* Use the `feature` tag, if your changes contain a new feature.
* Use the `refactor`tag, if your changes contain code cleanup or refactoring.
* Use separate PRs instead of mixing tags when possible. This reduces noise in PRs and makes them easier to review.

### Exceptions and Changes

These are not set in stone, fixed rules that never allow exceptions or that cannot be changed. Exceptions to the guidelines above will be considered on a case-by-base basis.

Changes to the guidelines may happen from time to time as well. To avoid the potential for excessive bike shedding, proposing changes are typically restricted to contributors that have submitted pull requests. This can be done by creating an issue in the project.
