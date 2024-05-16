// action = (item,next) => {}
export const AsyncForeach = async (collection, action, whenDone) => {
    for (var item of collection) {
        var next = null;
        var promise = new Promise((resolve) => { next = resolve });
        action(item, next);
        await promise;
    }
    if (whenDone)
        whenDone();
    console.log("AsyncForeach done");
}

//export const AsyncActions = async (actions) => {
//    for (var action of actions) {
//        var next = null;
//        var promise = new Promise((resolve) => { next = resolve });
//        action(next);
//        await promise;
//    }
//}