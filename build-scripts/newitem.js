const { exec } = require("child_process")
const { readFileSync, writeFileSync } = require("fs")

if (!process.argv || process.argv.length !== 4) {
  console.error('Need two args: postType and name', process.argv)
  return -1
}

const postType = process.argv[2]
const postName = process.argv[3]

const date = new Date()
const formatted = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`

const formattedName = postName
  .replace(/:/g, "-")
  .replace(/\s/g, "-")
  .replace(/--/g, "-")
  .toLowerCase()

const indexFileName = `${postType}s/${formatted}-${formattedName}`
const execString = `hugo new --kind ${postType}-bundle ${indexFileName}`

console.log(execString)

exec(execString, () => {
  // Callback after completed
  // Open file, replace title with the passed in postName

  const filename = `./content/${indexFileName}/index.md`
  const content = readFileSync(filename, { encoding: 'utf-8'})

  const newContent = content.replace("REPLACE_TITLE", postName)

  writeFileSync(filename, newContent)
})