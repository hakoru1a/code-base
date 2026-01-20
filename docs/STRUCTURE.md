# TL_BIOMASS - Project Structure Documentation

---

## Directory Structure

```
TL_BIOMASS/
├── docs/                     # Documentation files
│   └── STRUCTURE.md          # This file
├── dist/                     # Build output directory
├── node_modules/             # Project dependencies
├── public/                   # Static public assets
├── src/                      # Source code root
│   ├── assets/               # Static assets (images, styles, third-party libs)
│   ├── components/           # Reusable UI components of system
│   ├── contexts/             # React Context providers
│   ├── forms/                # Form-related components and utilities
│   ├── hooks/                # Custom React hooks
│   ├── layout/               # Layout components (auth, guest)
│   ├── libs/                 # External library configurations
│   ├── locales/              # Internationalization (i18n) files
│   ├── menu-items/           # Navigation menu configuration
│   ├── pages/                # Page components
│   ├── routes/               # Routing configuration
│   ├── services/             # API services layer
│   ├── themes/               # Theme customization
│   ├── utils/                # Utility functions
│   ├── App.tsx               # Root application component
│   ├── index.tsx             # Application entry point
│   └── types.ts              # Global type definitions
├── .eslintrc.cjs             # ESLint configuration
├── .env                      # Environment variables
├── .gitignore                # Git ignore rules
├── .prettierrc               # Prettier formatting rules
├── index.html                # HTML template
├── package.json              # Project dependencies and scripts
├── tsconfig.json             # TypeScript configuration
└── vite.config.ts            # Vite build configuration
```