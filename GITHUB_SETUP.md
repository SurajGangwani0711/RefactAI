# GitHub Setup Guide for RefactAI

## 🚀 Quick Upload to GitHub

### Step 1: Create a New Repository on GitHub

1. Go to https://github.com/new
2. Enter repository details:
   - **Repository name**: `RefactAI` (or your preferred name)
   - **Description**: AI-Powered Automated Code Refactoring with Microsoft Orleans
   - **Visibility**: Choose Public or Private
   - **DO NOT** initialize with README, .gitignore, or license (we already have these)
3. Click "Create repository"

### Step 2: Initialize Git and Push (Run these commands)

```bash
# Navigate to project directory
cd /Users/suraj/Documents/Personal_project/RefactAI

# Initialize git repository
git init

# Add all files (secrets/ will be ignored by .gitignore)
git add .

# Create first commit
git commit -m "$(cat <<'EOF'
Initial commit: RefactAI - AI-Powered Code Refactoring

- Multi-language support (C#, JS, TS, Java, Python, Go, Rust, etc.)
- Microsoft Orleans distributed architecture
- Local AI inference with Ollama (DeepSeek Coder)
- Automated GitHub PR creation
- React-based web UI
- Complete documentation and presentations

🤖 Generated with Claude Code
EOF
)"

# Add remote repository (REPLACE with your GitHub username and repo name)
git remote add origin https://github.com/YOUR_USERNAME/RefactAI.git

# Push to GitHub
git branch -M main
git push -u origin main
```

### Step 3: Verify Upload

1. Visit your repository: `https://github.com/YOUR_USERNAME/RefactAI`
2. Verify files are uploaded
3. Check that `secrets/` directory is NOT visible (it's in .gitignore)

---

## ⚠️ Security Checklist

Before pushing, verify these files are NOT included:

- [ ] ❌ `secrets/github.token` - GitHub token (SHOULD BE IGNORED)
- [ ] ❌ `secrets/Repo.url` - Repository URL (SHOULD BE IGNORED)
- [ ] ❌ `Token.json` - Any token files (SHOULD BE IGNORED)
- [ ] ❌ `bin/` and `obj/` directories (SHOULD BE IGNORED)
- [ ] ❌ `node_modules/` directory (SHOULD BE IGNORED)

To verify what will be committed:
```bash
git status
```

The `.gitignore` file will automatically exclude these sensitive files.

---

## 📝 Customizing Before Upload

### Update README.md

Replace placeholder information:
- Your email address
- Your LinkedIn profile
- Your actual GitHub repository URL
- Any specific setup instructions for your environment

### Update Presentations

In the pitch deck (`RefactAI_Pitch_Deck.pptx`):
- Slide 10: Add your contact information
- Slide 8: Specify funding amount (if applicable)

---

## 🔄 After Initial Upload

### To update your repository later:

```bash
# Stage all changes
git add .

# Commit with a descriptive message
git commit -m "Description of your changes"

# Push to GitHub
git push
```

### To create a new branch:

```bash
# Create and switch to new branch
git checkout -b feature/your-feature-name

# Push branch to GitHub
git push -u origin feature/your-feature-name
```

---

## 🌟 Recommended GitHub Repository Settings

### 1. Add Topics (for discoverability)

Add these topics to your repository:
- `ai`
- `code-refactoring`
- `microsoft-orleans`
- `ollama`
- `deepseek`
- `dotnet`
- `react`
- `github-integration`
- `automated-refactoring`
- `code-quality`

### 2. Add Repository Description

```
AI-powered automated code refactoring system using Microsoft Orleans,
Ollama, and GitHub integration. Local-first processing with support
for 10+ programming languages.
```

### 3. Add Website (optional)

If you deploy the UI somewhere, add the URL here.

### 4. Enable Issues

Enable Issues for bug tracking and feature requests.

### 5. Add License

Consider adding a license file:
- MIT License (permissive, popular for open source)
- Apache 2.0 (includes patent grant)
- GPL v3 (copyleft)

---

## 🔒 Security Best Practices

### Never Commit These:

1. **GitHub Tokens**: Always use environment variables or local files
2. **API Keys**: Use .env files (added to .gitignore)
3. **Passwords**: Never hardcode credentials
4. **Personal Data**: User information, emails, etc.

### If You Accidentally Commit Secrets:

1. **Immediately revoke the token** on GitHub
2. Generate a new token
3. Remove from Git history:
```bash
# Use BFG Repo-Cleaner or git filter-branch
# This is complex - better to revoke token immediately
```

---

## 📚 Additional Files to Consider

### LICENSE

Create a `LICENSE` file with your chosen license.

Example for MIT License:
```
MIT License

Copyright (c) 2024 [Your Name]

Permission is hereby granted, free of charge, to any person obtaining a copy...
```

### CONTRIBUTING.md

Guidelines for contributors:
- How to set up development environment
- Code style guidelines
- How to submit pull requests
- Testing requirements

### CODE_OF_CONDUCT.md

Community guidelines for collaboration.

---

## 🎯 Making Your Repository Stand Out

### 1. Add Badges to README.md

```markdown
![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)
![React](https://img.shields.io/badge/React-19.2-61DAFB?logo=react)
![Orleans](https://img.shields.io/badge/Orleans-9.2-7B68EE)
![License](https://img.shields.io/badge/license-MIT-green)
```

### 2. Add Screenshots

Create a `screenshots/` directory with:
- Web UI screenshot
- Architecture diagram
- Example PR screenshot
- Before/After code comparison

### 3. Add a Demo Video

Record a quick demo and link it in the README:
```markdown
## Demo

[![Watch the demo](thumbnail.png)](https://youtu.be/your-video-id)
```

### 4. GitHub Actions (CI/CD)

Create `.github/workflows/build.yml` for automated testing:
```yaml
name: Build and Test

on: [push, pull_request]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: 9.0.x
      - name: Restore dependencies
        run: dotnet restore
      - name: Build
        run: dotnet build --no-restore
      - name: Test
        run: dotnet test --no-build --verbosity normal
```

---

## 🚨 Common Issues and Solutions

### Issue: "Permission denied (publickey)"

**Solution:**
Use HTTPS instead of SSH:
```bash
git remote set-url origin https://github.com/USERNAME/RefactAI.git
```

### Issue: "Failed to push some refs"

**Solution:**
Pull first, then push:
```bash
git pull origin main --rebase
git push
```

### Issue: "Large files warning"

**Solution:**
Add large files to `.gitignore` or use Git LFS:
```bash
git lfs install
git lfs track "*.pptx"
git add .gitattributes
```

### Issue: ".gitignore not working"

**Solution:**
Clear git cache:
```bash
git rm -r --cached .
git add .
git commit -m "Fix .gitignore"
```

---

## 📞 Support

If you encounter issues:
1. Check GitHub's documentation: https://docs.github.com
2. Review git documentation: https://git-scm.com/doc
3. Open an issue in your repository

---

## ✅ Final Checklist

Before pushing to GitHub:

- [ ] `.gitignore` file created and excludes secrets
- [ ] Verified `git status` doesn't show sensitive files
- [ ] README.md is complete and accurate
- [ ] Contact information updated in presentations
- [ ] GitHub repository created
- [ ] Remote added: `git remote add origin ...`
- [ ] Initial commit created
- [ ] Pushed to GitHub: `git push -u origin main`
- [ ] Verified files on GitHub
- [ ] Secrets are NOT visible on GitHub
- [ ] Repository settings configured (description, topics)

---

**Your RefactAI project is ready to share with the world! 🚀**
