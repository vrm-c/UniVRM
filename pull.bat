:: pushd Assets\VRM
git pull
git submodule init
git submodule update --recursive --remote
git submodule foreach --recursive git checkout master
:: popd
pause
