import os

directory = r"d:\Repos\OmniMarket"
extensions = ('.cs', '.xaml', '.html', '.razor')

# We'll programmatically find the corruption for these missing symbols
symbols = ['₺', '🛒', '✎', '🗑', '🔍', '✕', '🥤', '🔄', '💾', '📦', '👥', '📊', '⚙', '🚪', '⚙️', '🧹', '📈', '📉', '💰', '➕', '➖', '✅', '❌']

corruptions = {}
for encoding in ['latin1', 'cp1254', 'cp1252']:
    for c in symbols:
        try:
            corrupted = c.encode('utf-8').decode(encoding)
            if corrupted != c:
                corruptions[corrupted] = c
        except Exception:
            pass

# Add known manual overrides if some encodings got lost or slightly altered
manual_overrides = {
    "â‚º": "₺",
    "ğŸ›’": "🛒",
    "âœ ": "✎",
    "ğŸ—‘": "🗑",
    "ğŸ” ": "🔍",
    "âœ•": "✕",
    "ğŸ¥¤": "🥤",
    "ğŸ”„": "🔄",
    "ğŸ’¾": "💾"
}

for k, v in manual_overrides.items():
    corruptions[k] = v

print(f"Total symbol corruptions to check: {len(corruptions)}")

def fix_file(filepath):
    try:
        with open(filepath, 'r', encoding='utf-8') as f:
            content = f.read()
            
        modified = False
        # Do lengths of keys from longest to shortest
        for k in sorted(corruptions.keys(), key=len, reverse=True):
            if k in content:
                content = content.replace(k, corruptions[k])
                modified = True
                
        if modified:
            with open(filepath, 'w', encoding='utf-8-sig') as f:
                f.write(content)
            return True
        return False
    except Exception as e:
        return False

count = 0
for root, dirs, files in os.walk(directory):
    if any(p in root.split(os.sep) for p in ['obj', 'bin', '.git', '.vs']):
        continue
    for file in files:
        if file.endswith(extensions):
            path = os.path.join(root, file)
            if fix_file(path):
                print(f"Fixed symbols in: {path}")
                count += 1

print(f"Total symbol fixes applied to {count} files.")
