#!/usr/bin/env python3

import os
import re
import sys
import requests
from tqdm import tqdm
from urllib.parse import urlparse

def find_markdown_files(root_dir):
    """
    Recursively find all Markdown files in the given directory.
    """
    md_files = []
    for dirpath, dirnames, filenames in os.walk(root_dir):
        for filename in filenames:
            if filename.lower().endswith('.md'):
                md_files.append(os.path.join(dirpath, filename))
                
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

def check_links(md_files, include_external = False):
    """
    Check all links in the given Markdown files and return a list of broken links.
    """
    broken_links = []
    session = requests.Session()
    headers = {'User-Agent': 'Mozilla/5.0 (LinkChecker/1.0)'}

    links_inspected_count = 0

    for md_file in md_files:
        links = extract_links_from_markdown(md_file)
        md_dir = os.path.dirname(md_file)

        for link in tqdm(links):
            link = link.strip()

            # Skip empty links and anchors
            if not link or link.startswith('#'):
                continue

            # Skip mailto and tel links
            if link.startswith(('mailto:', 'tel:')):
                continue

            if is_external_link(link):
                # Skip inspection of the link if we are not supposed to.
                if include_external is False:
                  continue
                
                links_inspected_count += 1
                
                # Check external link
                try:
                    response = session.head(link, allow_redirects=True, timeout=10, headers=headers)
                    if response.status_code >= 400 or response.status_code == 405:
                        # Try GET if HEAD is not allowed
                        response = session.get(link, allow_redirects=True, timeout=10, headers=headers)
                        if response.status_code >= 400:
                            broken_links.append((md_file, link, response.status_code))
                except requests.exceptions.RequestException as e:
                    broken_links.append((md_file, link, str(e)))
            else:
              links_inspected_count += 1
              # Check internal link
              link_path = link.split('#')[0]  # Remove any anchor
              abs_path = os.path.normpath(os.path.join(md_dir, link_path))
              # strip any leading '/' and '\' characters
              abs_path = abs_path.lstrip("/\\")
              if not os.path.exists(abs_path):
                  broken_links.append((md_file, link, abs_path, 'File not found'))

    return broken_links, links_inspected_count

def main():
    root_dir = 'com.playeveryware.eos'
    md_files = find_markdown_files(root_dir)
    broken_links, links_inspected = check_links(md_files)

    if broken_links:
        print("Broken links found:")
        for md_file, link, abs_link, error in broken_links:
            print(f"In file: {md_file}")
            print(f" -          Link: {link}")
            print(f" - Absolute Link: {abs_link}")
            print(f" - Error: {error}")
        print(f"A total of {len(broken_links)} broken links were identified out of {links_inspected} inspected.")
        sys.exit(1)
    else:
        print(f"No broken links found ({links_inspected} links were inspected).")
        sys.exit(0)

if __name__ == "__main__":
    main()
