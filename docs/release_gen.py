#
# Release 時のドキュメントを作成するスクリプト
#
# required
#   pip install GitPython pyperclip
#
# 1. github release page の markdown => clipboard
# 2. changelog => docs/release/079/v0.XX.Y.md
# 3. download button => docs/index.html
#
import pathlib
import re
import git.repo
import re
import pathlib
import io
from functools import cmp_to_key

HERE = pathlib.Path(__file__).absolute().parent
UNIVRM_VERSION = HERE.parent / "Assets/VRM/Runtime/Format/VRMVersion.cs"
MERGE_PATTERN = re.compile(r"Merge pull request #(\d+)")
TEMPLATE = HERE / "release_template.md"
HTML_TEMPLATE = HERE / "html_template.html"
RELEASE_NOTE_DIR = "112"
UNITY_VERSION = "2021.3 LTS"


def gen(template: str, version: str, hash: str):
    version_hash = f"{version}_{hash[0:4]}"
    values = {
        "version": version,
        "version_hash": version_hash,
        "hash4": version_hash[0:4],
        "dir": RELEASE_NOTE_DIR,
        "unity_version": UNITY_VERSION,
    }

    def replace(m: re.Match):
        return values[m.group(1)]

    return re.sub(r"\$\{([^}]+)\}", replace, template)


def get_version() -> str:
    m = re.search(
        r'public const string VERSION = "(\d.\d+.\d)";',
        UNIVRM_VERSION.read_text(encoding="utf-8"),
    )
    if m:
        return m[1]
    raise Exception("no version")


def get_hash(repo, tag_name) -> str:
    # res = subprocess.check_output("git rev-parse HEAD")
    # return res.decode('utf-8')
    for tag in repo.tags:
        if tag.name == tag_name:
            return tag.commit.hexsha
    raise Exception()


def copy_release_md(version: str, hash: str):
    import pyperclip

    text = gen(TEMPLATE.read_text(encoding="utf-8"), version, hash)
    pyperclip.copy(text)
    print("copy to clipboard")


#
#
#
def change_log(repo: git.repo.Repo, version: str):
    major, minor, patch = [int(x) for x in version.split(".")]
    rev = f"v{major}.{minor-1}.0..v{major}.{minor}.0"

    w = io.StringIO()
    w.write(f"# {rev}: ChangeLog\n")
    w.write("\n")
    for item in repo.iter_commits(rev=rev):
        if len(item.parents) > 1:
            msg = item.message
            if isinstance(msg, bytes):
                msg = msg.decode("utf-8")
            m = MERGE_PATTERN.match(msg)
            if m:
                # merge commit
                pr = m[1]
                lines = msg.split("\n")

                w.write(
                    f"* [[\\#{pr}](https://github.com/vrm-c/UniVRM/pull/{pr})] {lines[2]}\n"
                )
            else:
                w.write(f"* {msg}")
    return w.getvalue()


def get_tags(repo):
    P = re.compile(r"v(\w+)\.(\w+)\.(\w+)")
    for tag in repo.tags:
        m = P.match(tag.name)
        if m:
            yield int(m[1]), int(m[2]), int(m[3])


if __name__ == "__main__":
    repo = git.repo.Repo(str(HERE.parent))

    def cmp_tag(l, r):
        if l[0] != r[0]:
            return l[0] - r[0]
        if l[1] != r[1]:
            return l[1] - r[1]
        return l[2] - r[2]

    tags = [tag for tag in get_tags(repo)]
    tags = sorted(tags, key=cmp_to_key(cmp_tag))
    x, y, z = tags[-1]
    version = f"{x}.{y}.{z}"
    hash = get_hash(repo, f"v{version}")
    # 1.
    copy_release_md(f"{version}", hash)
    # 2.
    release = HERE / f"release/{RELEASE_NOTE_DIR}/v{version}.md"
    if not release.exists():
        text = change_log(repo, f"{version}")
        release.write_text(text, encoding="utf-8")
    # 3.
    (HERE / "index.html").write_text(
        gen(HTML_TEMPLATE.read_text(encoding="utf-8"), f"{version}", hash),
        encoding="utf-8",
    )
