###############################################################################
#
# install.ps1 -- Set 'sqlite3.dll' Build action to Copy to output directory
#
###############################################################################

param($installPath, $toolsPath, $package, $project)

$fileName = "sqlite3.dll"
$propertyName = "CopyToOutputDirectory"

$item = $project.ProjectItems.Item($fileName)

if ($item -eq $null) {
  continue
}

$property = $item.Properties.Item($propertyName)

if ($property -eq $null) {
  continue
}

$property.Value = 2
