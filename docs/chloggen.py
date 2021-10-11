# -*- coding: utf-8 -*-
import sys
import git.repo
import re
import pathlib

HERE = pathlib.Path(__file__).absolute().parent

MERGE_PATTERN = re.compile(r'Merge pull request #(\d+)')


def main(repo: git.repo.Repo, rev: str):
    print('# v0.86.0: 1.0準備')
    print()
    for item in repo.iter_commits(rev=rev):
        m = MERGE_PATTERN.match(item.message)
        if m:
            # merge commit
            pr = m[1]
            lines = item.message.splitlines()

            print(
                f'* [[\\#{pr}](https://github.com/vrm-c/UniVRM/pull/{pr})] {lines[2]}'
            )


if __name__ == '__main__':
    repo = git.repo.Repo(str(HERE.parent))
    main(repo, sys.argv[1])
