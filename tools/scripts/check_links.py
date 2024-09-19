#!/usr/bin/env python3

import os
import re
import sys
import logging
import requests
import argparse
import json
from tqdm import tqdm
from urllib.parse import urlparse
from pathlib import Path
from concurrent.futures import ThreadPoolExecutor, as_completed
from bs4 import BeautifulSoup  # Import BeautifulSoup

# Set up logging
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')

# Update this array with links that are acceptable to fail inspection
bypass_inspection_links = (
    'http://localhost:8080',
    'https://eoshelp.epicgames.com/'
)

# Cache for file encoding detection (optional, for handling encoding issues)
file_encoding_cache = {}

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
    Extract all links and image sources from a Markdown file,
    including those in HTML tags.
    """
    try:
        encoding = file_encoding_cache.get(md_file, 'utf-8')
        with open(md_file, 'r', encoding=encoding, errors='ignore') as f:
            content = f.read()
    except Exception as e:
        logging.warning(f"Error reading file {md_file}: {e}")
        return []

    # Regex patterns to find Markdown links and images
    link_pattern = re.compile(r'(?<!\!)\[(?:[^\]]+)\]\(([^)]+)\)')
    image_pattern = re.compile(r'\!\[(?:[^\]]*)\]\(([^)]+)\)')

    # Find Markdown links and images
    links = link_pattern.findall(content)
    images = image_pattern.findall(content)

    # Parse HTML content
    soup = BeautifulSoup(content, 'html.parser')

    # Extract href attributes from <a> tags
    html_links = [a.get('href') for a in soup.find_all('a', href=True)]

    # Extract src attributes from <img> tags
    html_images = [img.get('src') for img in soup.find_all('img', src=True)]

    # Combine all links
    all_links = links + images + html_links + html_images

    # Filter out None values and duplicates
    all_links = list(set(filter(None, all_links)))

    return all_links

def is_external_link(url):
    """
    Check if a URL is an external link.
    """
    parsed_url = urlparse(url)
    return parsed_url.scheme in ('http', 'https', 'ftp')

def get_md_files_to_links(md_files, include_external):
    """
    Create a dictionary mapping Markdown files to their extracted links.
    """
    md_files_to_links = {}
    for md_file in md_files:
        links_in_md_file = extract_links_from_markdown(md_file)
        links_to_inspect = []
        md_dir = os.path.dirname(md_file)

        for link in links_in_md_file:
            link = link.strip()

            if link in bypass_inspection_links:
                continue
            if not link or link.startswith('#'):
                continue
            if link.startswith(('mailto:', 'tel:')):
                continue
            if is_external_link(link) and not include_external:
                continue

            links_to_inspect.append(link)

        links_to_inspect = list(set(links_to_inspect))
        md_files_to_links[md_file] = links_to_inspect

    return md_files_to_links

def compute_abs_link(md_dir, link):
    """
    Compute the absolute path of a link, handling anchors and normalizing the path.
    """
    link_path = link.split('#')[0]
    abs_path = os.path.normpath(os.path.join(md_dir, link_path))
    abs_path = abs_path.lstrip("/\\")
    return abs_path

def check_link(session, link, md_file, headers, link_inspection_results):
    """
    Inspect a single link, returning the result.
    """
    md_dir = os.path.dirname(md_file)
    result = None
    abs_link = link

    # Cache key includes md_dir for internal links to avoid conflicts
    cache_key = link if is_external_link(link) else os.path.normpath(os.path.join(md_dir, link))

    # Check if the link has already been inspected
    if cache_key in link_inspection_results:
        result = link_inspection_results[cache_key]
        if not is_external_link(link):
            abs_link = compute_abs_link(md_dir, link)
    else:
        if is_external_link(link):
            try:
                response = session.head(link, allow_redirects=True, timeout=10, headers=headers)
                if response.status_code != 403 and (response.status_code >= 400 or response.status_code == 405):
                    response = session.get(link, allow_redirects=True, timeout=10, headers=headers)
                    if response.status_code >= 400 and response.status_code != 403:
                        result = response.status_code
            except requests.exceptions.RequestException as e:
                result = str(e)
        else:
            abs_link = compute_abs_link(md_dir, link)
            if not os.path.exists(abs_link) and not os.path.exists(link):
                result = 'File not found'

        link_inspection_results[cache_key] = result

    return result, abs_link, md_file, link

def check_links(md_files, include_external=False, concurrent_requests=10):
    """
    Check all links in the given Markdown files and return a list of broken links.
    """
    broken_links = []
    session = requests.Session()
    headers = {'User-Agent': 'Mozilla/5.0 (LinkChecker/1.0)'}

    md_files_to_links = get_md_files_to_links(md_files, include_external)

    link_inspection_results = {}
    links_to_inspect = sum(len(links) for links in md_files_to_links.values())
    links_inspected_count = 0

    def generate_items():
        for md_file, link_list in md_files_to_links.items():
            for link in link_list:
                yield md_file, link

    with tqdm(total=links_to_inspect, desc="Inspecting links") as pbar:
        with ThreadPoolExecutor(max_workers=concurrent_requests) as executor:
            futures = []
            for md_file, link in generate_items():
                futures.append(executor.submit(check_link, session, link, md_file, headers, link_inspection_results))

            for future in as_completed(futures):
                result, abs_link, md_file, link = future.result()
                if result is not None:
                    broken_links.append((md_file, link, abs_link, result))
                links_inspected_count += 1
                pbar.set_postfix(broken=len(broken_links))
                pbar.update(1)

    return broken_links, links_inspected_count

def main():
    parser = argparse.ArgumentParser(description="Link checker for Markdown files.")
    parser.add_argument("--root-dir", type=str, default=".", help="Root directory to search for Markdown files")
    parser.add_argument("--include-external", action="store_true", help="Include external links in the inspection")
    parser.add_argument("--concurrent-requests", type=int, default=10, help="Number of concurrent requests for external links")
    parser.add_argument("--output-format", type=str, choices=["json", "text"], default="text", help="Output format (json or text)")

    args = parser.parse_args()

    root_dir = args.root_dir
    md_files = find_markdown_files(root_dir)
    broken_links, links_inspected = check_links(md_files, include_external=args.include_external, concurrent_requests=args.concurrent_requests)

    if broken_links:
        if args.output_format == "json":
            output = {
                "broken_links": [
                    {
                        "file": md_file,
                        "link": link,
                        "absolute_link": abs_link,
                        "error": error
                    } for md_file, link, abs_link, error in broken_links
                ],
                "total_links_inspected": links_inspected
            }
            print(json.dumps(output, indent=2))
        else:
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
        print(f"No broken links found. A total of {links_inspected} links were inspected.")
        sys.exit(0)

if __name__ == "__main__":
    main()
