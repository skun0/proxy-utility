# LEAVE A STAR TO THE PROJECT IF YOU ENJOY IT!  

import requests
import os
from colorama import Fore
import concurrent.futures
import ctypes
ctypes.windll.kernel32.SetConsoleTitleW("Proxy Utility by Skuno")
logo = Fore.GREEN + '''
            ██████╗ ██████╗  ██████╗ ██╗  ██╗██╗   ██╗    ██╗   ██╗████████╗██╗██╗     ██╗████████╗██╗   ██╗
            ██╔══██╗██╔══██╗██╔═══██╗╚██╗██╔╝╚██╗ ██╔╝    ██║   ██║╚══██╔══╝██║██║     ██║╚══██╔══╝╚██╗ ██╔╝
            ██████╔╝██████╔╝██║   ██║ ╚███╔╝  ╚████╔╝     ██║   ██║   ██║   ██║██║     ██║   ██║    ╚████╔╝ 
            ██╔═══╝ ██╔══██╗██║   ██║ ██╔██╗   ╚██╔╝      ██║   ██║   ██║   ██║██║     ██║   ██║     ╚██╔╝  
            ██║     ██║  ██║╚██████╔╝██╔╝ ██╗   ██║       ╚██████╔╝   ██║   ██║███████╗██║   ██║      ██║   
            ╚═╝     ╚═╝  ╚═╝ ╚═════╝ ╚═╝  ╚═╝   ╚═╝        ╚═════╝    ╚═╝   ╚═╝╚══════╝╚═╝   ╚═╝      ╚═╝                           
                                            https://github.com/skun0                                                                    
'''

menu = Fore.GREEN + '''
    / Proxies /
    1- Proxy Scraper
    2- Proxy Checker
    3- Exit
'''

def Clear():
    os.system("cls" if os.name == "nt" else "clear")

def proxy_scraper():
    Clear()
    print(logo)
    print("Scraping proxies...")
    urls = [
        "https://api.proxyscrape.com/v4/free-proxy-list/get?request=display_proxies&proxy_format=ipport&format=text&timeout=20000",
        "https://raw.githubusercontent.com/ProxyScraper/ProxyScraper/refs/heads/main/http.txt",
        "https://raw.githubusercontent.com/ProxyScraper/ProxyScraper/refs/heads/main/socks5.txt"
    ]
    proxies = ""
    for url in urls:
        try:
            response = requests.get(url, timeout=10)
            if response.status_code == 200:
                proxies += response.text.strip() + "\n"
            else:
                print(f"Failed to fetch proxies from {url}")
        except Exception as e:
            print(f"Error fetching {url}: {e}")

    proxies = proxies.strip()
    with open('proxies.txt', 'w') as f:
        f.write(proxies)
    print(f"Scraped {len(proxies.splitlines())} proxies and saved to proxies.txt")
    input("Press enter to return to menu...")

def proxy_checker():
    Clear()
    print(logo)
    try:
        with open('proxies.txt', 'r') as f:
            proxy_list = [line.strip() for line in f if line.strip()]
    except FileNotFoundError:
        print("No proxies.txt file found.")
        input("Press enter to return to menu...")
        return

    print(f"Started checking {len(proxy_list)} proxies...")

    valid_proxies = []
    invalid_counter = [0]
    total_checked = [0]

    def check_proxy(proxy):
        try:
            proxies = {
                "http": f"http://{proxy}",
                "https": f"http://{proxy}"
            }
            r = requests.get("https://api.myip.com/", proxies=proxies, timeout=5)
            total_checked[0] += 1
            if r.status_code == 200:
                title = f"Checking: {proxy} [VALID] - Checked: {total_checked[0]}/{len(proxy_list)}"
                ctypes.windll.kernel32.SetConsoleTitleW(title)
                print(Fore.GREEN + f"[VALID] {proxy}" + Fore.RESET)
                return proxy
            else:
                invalid_counter[0] += 1
                title = f"Checking: {proxy} [INVALID] - Checked: {total_checked[0]}/{len(proxy_list)}"
                ctypes.windll.kernel32.SetConsoleTitleW(title)
                print(Fore.RED + f"[INVALID] {proxy}" + Fore.RESET)
                return None
        except Exception:
            invalid_counter[0] += 1
            total_checked[0] += 1
            title = f"Checking: {proxy} [INVALID] - Checked: {total_checked[0]}/{len(proxy_list)}"
            ctypes.windll.kernel32.SetConsoleTitleW(title)
            print(Fore.RED + f"[INVALID] {proxy}" + Fore.RESET)
            return None

    with concurrent.futures.ThreadPoolExecutor(max_workers=30) as executor:
        results = executor.map(check_proxy, proxy_list)

    valid_proxies = [p for p in results if p]
    invalid_count = invalid_counter[0]

    with open('valid_proxies.txt', 'w') as f:
        f.write("\n".join(valid_proxies))

    ctypes.windll.kernel32.SetConsoleTitleW("Proxy Utility by Skuno")
    print(f"\nValid proxies: {len(valid_proxies)}")
    print(f"Invalid proxies: {invalid_count}.")
    print(f"Valid proxies has been saved to valid_proxies.txt")
    input("Press enter to return to menu...")

def main():
    while True:
        Clear()
        print(logo)
        print(menu)
        choice = input("-> " + Fore.RESET).strip()
        if choice == "1":
            proxy_scraper()
        elif choice == "2":
            proxy_checker()
        elif choice == "3":
            break
        else:
            print("Invalid choice.")
            input("Press enter to continue...")

if __name__ == "__main__":
    main()
