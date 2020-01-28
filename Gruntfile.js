var toml = require("yamljs");

var CONTENT_PATH_PREFIX = "content";

module.exports = function(grunt) {

    grunt.registerTask("build", function() {

        grunt.log.writeln("Build pages index");

        var indexPages = function() {
            var pagesIndex = [];
            grunt.file.recurse(CONTENT_PATH_PREFIX, function(abspath, rootdir, subdir, filename) {
                if (filename !== 'index.md') {
                    return;
                }
                grunt.verbose.writeln("Parse file:",abspath);
                var index = processFile(abspath, filename);
                if (index !== -1) {
                    pagesIndex.push(index);
                }
            });

            return pagesIndex;
        };

        var processFile = function(abspath, filename) {
            return processMDFile(abspath, filename);
        };

        var processMDFile = function(abspath, filename) {
            console.log(abspath);
            console.log(filename);
            var content = grunt.file.read(abspath);
            var pageIndex;
            // First separate the Front Matter from the content and parse it
            content = content.split("---");
            var frontMatter;
            try {
                frontMatter = toml.parse(content[1].trim());
            } catch (e) {
                console.error(e.message);
            }
            
            if (frontMatter.draft === true) {
                console.log('hereeeeeee');
                return -1;
            }

            // Build Lunr index for this page
            pageIndex = {
                title: frontMatter.title,
                tags: frontMatter.tags,
                href: '/post/' + frontMatter.slug
            };

            return pageIndex;
        };

        grunt.file.write("static/js/PagesIndex.json", JSON.stringify(indexPages()));
        grunt.log.ok("Index built");
    });
};