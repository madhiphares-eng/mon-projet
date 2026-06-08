# mon-projet — V8M Auto

Boutique en ligne **V8M Auto** propulsée par [Shopify](https://www.shopify.com/).

Ce dépôt regroupe les ressources du projet : personnalisation du thème, configuration
et documentation de la boutique.

## Aperçu

- **Plateforme :** Shopify
- **Type :** Boutique e-commerce
- **Statut :** En cours de développement

## Démarrage

### Prérequis

- Un compte [Shopify Partners](https://www.shopify.com/partners) ou une boutique Shopify
- [Shopify CLI](https://shopify.dev/docs/themes/tools/cli) installé

```bash
# Installer Shopify CLI (via Homebrew)
brew tap shopify/shopify
brew install shopify-cli
```

### Développement du thème

```bash
# Se connecter à la boutique
shopify auth login

# Lancer le serveur de développement local
shopify theme dev

# Pousser le thème vers la boutique
shopify theme push
```

## Structure du dépôt

```
mon-projet/
├── README.md      # Ce fichier
└── .gitignore     # Fichiers ignorés par Git
```

## Contribuer

Les contributions se font via des *pull requests* :

1. Créez une branche : `git checkout -b ma-fonctionnalite`
2. Commitez vos changements : `git commit -m "Description"`
3. Poussez la branche : `git push -u origin ma-fonctionnalite`
4. Ouvrez une pull request sur GitHub

## Licence

Ce projet est privé. Tous droits réservés.
