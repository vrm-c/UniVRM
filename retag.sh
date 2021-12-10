function retag()
{
    echo $1
    set -x
    # local
    if [ `git tag -l | grep -e $1 > /dev/null 2>&1` ];then
        # remove
        git tag -d $1
        git push upstream :$1
    fi
    git tag $1
    git push upstream --tags
    set +x
}

