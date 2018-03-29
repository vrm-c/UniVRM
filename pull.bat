::pushd Assets\VRM
git pull
git submodule init
::git submodule update --remote
git submodule foreach git checkout master
git submodule foreach git pull
::popd
pause
