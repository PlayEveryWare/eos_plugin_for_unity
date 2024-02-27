"""
Copyright (c) 2024 PlayEveryWare, Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
"""

import sys
import datetime
import re

def is_binary(file_path):
    """
    Checks if the specified file is binary.
    Reads a small portion of the file to check for null bytes.
    """
    try:
        with open(file_path, 'rb') as file:
            chunk = file.read(1024)  # Read the first 1024 bytes
            return b'\0' in chunk
    except IOError:
        print(f"Error opening or reading file: {file_path}")
        return False

def filter_non_binary_files(files):
    """
    Filters out binary files from a list of file paths.
    """
    return [file for file in files if not is_binary(file)]

def update_copyright_notice(file_path) -> bool:
    """
    Given a file, scans each line to find a copyright notice, and updates the year.
    """
    # Read the original content of the file
    try:
        with open(file_path, 'r', encoding='utf-8') as file:
            content = file.read()
    except IOError as e:
        print(f"Error opening or reading file: {file_path}. {e}")
        return
    
    current_year = datetime.datetime.now().year
    
    # Function to use in re.sub for replacement logic
    def replace_func(match) -> str:
        new_copyright = ""
        start_year = match.group(1)
        # If the copyright notice already has a range
        if match.group(2):  
            new_copyright = f"(c) {start_year}-{current_year} PlayEveryWare, Inc."
        else:
            if start_year == str(current_year):
                # If the start year is the current year, no range needed
                new_copyright = match.group(0)
            else:
                # Update the year to a range from the original year to the current year
                new_copyright = f"(c) {start_year}-{current_year} PlayEveryWare, Inc."
        
        return new_copyright
    
    pattern = re.compile(r'\(c\) (\d{4})(|-\d{4})? PlayEveryWare.*')
    
    # Update the content
    updated_content = pattern.sub(replace_func, content)
    
    # If the content hasn't changed, don't do anything
    if updated_content == content:
        print(f'Copyright either missing or up-to-date in file "{file_path}"')
        return False
    
    # Write the updated content back to the file
    try:
        with open(file_path, 'w', encoding='utf-8') as file:
            file.write(updated_content)
        print(f"Updated copyright notice in {file_path}")
    except IOError as e:
        print(f"Error writing to file: {file_path}. {e}")
        return False
    
    return True

def main(changed_files):
    # Split by newline and strip each entry of quotation marks
    files = [f.strip('\'"') for f in changed_files.split('\n', maxsplit=-1)]
    
    # Filter out empty strings in case there are any
    files = list(filter(None, files))
    
    for f in files:
        print(f'File: "{f}"')
    
    # Limit the files to files that are not binary files
    files = filter_non_binary_files(files)
    
    print("\nNon-binary files:")
    files_were_changed = False

    for f in files:
        if update_copyright_notice(f):
            print(f'File "{f}" had it\'s copyright updated.')
            files_were_changed = True
            
    if files_were_changed:
        sys.exit(0) # files were changed successfully
    else:
        sys.exit(1) # indicates that no files were altered
    
if __name__ == "__main__":
    """
    This Python script expects a new-line delimited list of filepaths, optionally
    filepaths can be enclosed in quotes. This script does NOT try to determine if
    the file was changed - it is expected that all files passed to this script
    should have their copyright year or year range updated. The logic for that
    component can be found in the GitHub workflow file that references this script.
    """
    changed_files = sys.argv[1]
    main(changed_files)    
