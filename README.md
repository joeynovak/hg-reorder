# hg-reorder
A tool for re-ordering mercurial changesets in local repositories.  Tool is written in C#

This tool is used by creating an empty mercurial repository, and in the main dialog setting a source and destination repo.  The tool dumps a log of the source repository and will then pull these revisions in the order they are in the source repository.  However, if the commit message for a commit matches a pattern specified in the patterns text box, that commit will be pulled immediatly after it's parent commit.

*Only local repos can be used*
