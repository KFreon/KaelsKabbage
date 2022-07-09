const { exec } = require("child_process")

if (!process.argv || process.argv.length !== 4) {
  console.error('Need two args: postType and name', process.argv)
  return -1
}

const postType = process.argv[2]
const postName = process.argv[3]

const date = new Date()
const formatted = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`

const execString = `hugo new --kind ${postType}-bundle ${postType}s/${formatted}-${postName}`

console.log(execString)

exec(execString)