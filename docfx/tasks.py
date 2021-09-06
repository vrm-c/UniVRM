from invoke import task, Context
import pathlib
import re
import shutil

HERE = pathlib.Path(__file__).absolute().parent
SRC_DIR = HERE / 'src/UniVRM'
ROOT = HERE.parent
SLN = ROOT / 'UniVRM.sln'
# Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "UniGLTF", "UniGLTF.csproj", "{c921a465-ccf0-1feb-baab-95ab09accc12}"
PROJECT_PATTERN = re.compile(
    r'Project\("[^"]+"\) = "[^"]+", "([^"]+\.csproj)",')
ASSETS_DIR = ROOT / 'Assets'


def process(csproj: pathlib.Path, dst: pathlib.Path):
    print(csproj, "=>", dst)
    body = csproj.read_text(encoding='utf-8')

    #  <Compile Include="Assets\MeshUtility\Editor\BoneMeshEraserWizard.cs" />
    #  <Compile Include="Assets\MeshUtility\Editor\HumanoidEditor.cs" />
    #  <Compile Include="Assets\MeshUtility\Editor\MeshUtility.cs" />

    body = body.replace('"Assets', '"..\\..\\..\\Assets')

    dst.write_text(body, encoding='utf-8')


@task
def copy_csproj(c):
    # type: (Context) -> None
    '''
    task description
    '''
    shutil.rmtree(SRC_DIR, ignore_errors=True)
    SRC_DIR.mkdir(parents=True, exist_ok=True)
    for l in SLN.read_text('utf-8').splitlines():
        m = PROJECT_PATTERN.search(l)
        if m:
            process(ROOT / m.group(1), SRC_DIR / m.group(1))


@task
def meta_build(c):
    # type: (Context) -> None
    '''
    task description
    '''
    c.run("docfx metadata")
    c.run("docfx build")
