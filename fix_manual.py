import os

directory = r'd:\Repos\OmniMarket'
extensions = ('.cs', '.xaml', '.html', '.razor')

manual = {
    'â† ': '←',
    'ğŸ“‹': '📋',
    'âœ ': '✎',
    'âœ•': '✕'
}

count = 0
for root, dirs, files in os.walk(directory):
    if any(p in root.split(os.sep) for p in ['obj', 'bin', '.git', '.vs']):
        continue
    for file in files:
        if file.endswith(extensions):
            path = os.path.join(root, file)
            try:
                with open(path, 'r', encoding='utf-8') as f:
                    content = f.read()
                modified = False
                for k, v in manual.items():
                    if k in content:
                        content = content.replace(k, v)
                        modified = True
                if modified:
                    with open(path, 'w', encoding='utf-8-sig') as f:
                        f.write(content)
                    print(f'Fixed: {path}')
                    count += 1
            except Exception as e:
                pass

print(f'Total files manual fixed: {count}')
