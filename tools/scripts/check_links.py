#!/usr/bin/env python3

import os
import re
import sys
import requests
from tqdm import tqdm
from urllib.parse import urlparse

# Update this array with links that are acceptable to fail inspection. When 
# Updating this array, be certain to give a detailed acount of why it is 
# appropriate that testing them be bypassed.
bypass_inspection_links = (
  # This is bypassed because it is used to describe steps in a process that the
  # user may not want - so we do not know for sure whether this should pass or
  # fail inspection.
  'http://localhost:8080',
  # There were some strange behaviors observed with this url in particular 
  # regarding max retries being exceeded - which is odd because this script
  # caches the inspection results - so only one call is made to this url.
  'https://eoshelp.epicgames.com/'
)

def find_markdown_files(root_dir):
    """
    Recursively find all Markdown files in the given directory.
    """
    md_files = []
    for dirpath, dirnames, filenames in os.walk(root_dir):
        for filename in filenames:
            if filename.lower().endswith('.md'):
                md_files.append(os.path.join(dirpath, filename))

    # Sort the files in descending order of size                
    md_files.sort(key=lambda x: os.path.getsize(x), reverse=True)
    
    return md_files

def extract_links_from_markdown(md_file):
    """
    Extract all links and image sources from a Markdown file.
    """
    with open(md_file, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Regex patterns to find links and images
    link_pattern = r'(?<!\!)\[(?:[^\]]+)\]\(([^)]+)\)'
    image_pattern = r'\!\[(?:[^\]]*)\]\(([^)]+)\)'

    links = re.findall(link_pattern, content)
    images = re.findall(image_pattern, content)

    return links + images

def is_external_link(url):
    """
    Check if a URL is an external link.
    """
    return urlparse(url).scheme in ('http', 'https')

def get_md_files_to_links(md_files, include_external):
  md_files_to_links = {}
  for md_file in md_files:
    links_in_md_file = extract_links_from_markdown(md_file)
    links_to_inspect = list()
    md_dir = os.path.dirname(md_file)
    
    for link in links_in_md_file:
      link = link.strip()

      # Skip links that are in the bypass list
      if link in bypass_inspection_links:
        continue

      # Skip empty links and anchors
      if not link or link.startswith('#'):
        continue
      
      # Skip mailto and tel links
      if link.startswith(('mailto:', 'tel:')):
        continue
      
      # Skip if we're not supposed to inspect external links
      if is_external_link(link) and include_external is False:
        continue
        
      # Otherwise add it to the list of links in this md file that need to be
      # inspected
      links_to_inspect.append(link.strip())
    
    # Make the list of links unique by making it a set and making the set a 
    # list
    links_to_inspect = list(set(links_to_inspect))
    
    # Add the list of links to inspect for the file to the hash table that 
    # maps a markdown file to the list of links
    md_files_to_links[md_file] = links_to_inspect
  return md_files_to_links

def check_links(md_files, include_external = False):
    """
    Check all links in the given Markdown files and return a list of broken links.
    """
    broken_links = []
    session = requests.Session()
    headers = {'User-Agent': 'Mozilla/5.0 (LinkChecker/1.0)'}

    links_inspected_count = 0

    md_files_to_links = get_md_files_to_links(md_files, include_external)

    # So that no link is inspected twice, keep track of the result of each link
    # inspection
    link_inspection_results = {}
    
    # Count of the number of links inspected (whether a cached result was)
    links_inspected_count = 0
    
    # Because we want to observe progress linearly, use a generator to iterate
    # over all the links instead of going key by key
    def generate_items():
      for md_file, link_list in md_files_to_links.items():
        for link in link_list:
          yield md_file, link
          
    # To measure progress, despite our use of the generator, we need a total
    # count of the links that need inspecting
    links_to_inspect = 0
    for md_file in md_files_to_links.keys():
      links_to_inspect += len(md_files_to_links[md_file])
    
    with tqdm(total=links_to_inspect, desc="Inspecting links") as pbar:
      
      def update_progress_bar():
        pbar_description = f"Inspecting links {links_inspected_count} / {links_to_inspect}"
        if len(broken_links) != 0:
          pbar_description += f" - ({len(broken_links)} broken)"
          
        pbar.set_description(pbar_description)

        pbar.update(1)
        
      for md_file, link in generate_items():
      
        md_dir = os.path.dirname(md_file)
        
        # Increment the count of links that have been inspected
        links_inspected_count += 1
        
        # If the link has already been inspected, then use the result of that
        # previous inspection.
        if link in link_inspection_results:
          # If the value is none, then there was nothing wrong with the link, and
          # we can continue to the next link
          if link_inspection_results[link] is None:
            # TODO: Refactor to reduce the number of times we update progress bar
            update_progress_bar()
            continue
          
          # Otherwise add to the broken links as appropriate
          if is_external_link(link):
            broken_links.append((md_file, link, link, link_inspection_results[link]))
          else:
            # TODO: Reduce duplicate code that does this same thing later in the file
            link_path = link.split('#')[0]
            abs_path = os.path.normpath(os.path.join(md_dir, link_path))
            abs_path = abs_path.lstrip("/\\")
            broken_links.append((md_file, link, abs_path, 'File not found'))

          # Now that we have added the cached result of the inspection to the list
          # of broken links, we can continue to the next link, but first we 
          # should update the progress bar
          update_progress_bar()
          continue
        
        # Inspect the link
        result = None
        # If the link is an external link
        if is_external_link(link):
          
          # If we are allowed to inspect external links
          if include_external is False:
            continue
          
          try:
            response = session.head(link, allow_redirects=True, timeout=10, headers=headers)
            
            # Note that 403 is okay because there are links to documentation and
            # tools on dev.epicgames.com that require authentication - so a 403
            # error code is not an example of a bad link.
            if response.status_code != 403 and (response.status_code >= 400 or response.status_code == 405):
              response = session.get(link, allow_redirects=True, timeout=10, headers=headers)
              if response.status_code >= 400 and response.status_code != 403:
                result = response.status_code
          except requests.exceptions.RequestException as e:
            result = str(e)
        
          if result is not None:
            broken_links.append((md_file, link, link, result))
            
        # If the link is not an external link
        else:
          # Remove any anchor, if there is one
          link_path = link.split('#')[0]
          # Normalize the path
          abs_path = os.path.normpath(os.path.join(md_dir, link_path))
          # strip any leading '/' and '\' characters
          abs_path = abs_path.lstrip("/\\")
          if not os.path.exists(abs_path):
            result = 'File not found'
            broken_links.append((md_file, link, abs_path, result))
          
        link_inspection_results[link] = result
        
        # Update the progress bar
        update_progress_bar()
        
    return broken_links, links_inspected_count

def main():
    root_dir = '.'
    md_files = find_markdown_files(root_dir)
    broken_links, links_inspected = check_links(md_files, True)

    if broken_links:
        print("Broken links found:")
        for md_file, link, abs_link, error in broken_links:
            print(f"In file: {md_file}")
            print(f" -          Link: {link}")
            if link != abs_link:
              print(f" - Absolute Link: {abs_link}")
            print(f" - Error: {error}")
        print(f"A total of {len(broken_links)} broken links were identified out of {links_inspected} inspected.")
        sys.exit(1)
    else:
        print(f"No broken links found.")
        sys.exit(0)

if __name__ == "__main__":
    main()
