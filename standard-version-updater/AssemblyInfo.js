module.exports.readVersion = function (contents) {
    var regex = /\[assembly: AssemblyVersion\(\"(.*)\"\)\]/g;
    return contents.match(regex)[0].replace(regex, "$1")
};

module.exports.writeVersion = function (contents, version) {
    var regex = /\[assembly: AssemblyVersion\(\"(.*)\"\)\]/g;
    return contents.replace(regex, `[assembly: AssemblyVersion("${version}")]`)
};