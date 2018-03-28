:: pushd Assets\VRM
git pull
git submodule init
git submodule foreach --recursive git checkout master
git submodule update --recursive --remote
:: popd
pause
