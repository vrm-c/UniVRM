# -*- coding: utf-8 -*-
import sys
import git.repo
import re
import pathlib
import release_gen
from typing import TextIO

HERE = pathlib.Path(__file__).absolute().parent

MERGE_PATTERN = re.compile(r'Merge pull request #(\d+)')


def main(w: TextIO, repo: git.repo.Repo, version: str):
    major, minor, patch = [int(x) for x in version.split('.')]
    rev = f'v{major}.{minor-1}.0..v{major}.{minor}.0'

    w.write(f'# v{version}: 1.0準備\n')
    w.write('\n')
    for item in repo.iter_commits(rev=rev):
        m = MERGE_PATTERN.match(item.message)
        if m:
            # merge commit
            pr = m[1]
            lines = item.message.splitlines()

            w.write(
                f'* [[\\#{pr}](https://github.com/vrm-c/UniVRM/pull/{pr})] {lines[2]}\n'
            )


if __name__ == '__main__':
    repo = git.repo.Repo(str(HERE.parent))
    version = release_gen.get_version()

    dst = HERE / f'release/079/v{version}.md'
    with dst.open('w', encoding='utf-8') as w:
        main(w, repo, version)
    print(f'write to: {dst}')
