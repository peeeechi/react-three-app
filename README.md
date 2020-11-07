# Electron with Typescript application example

このSampleは、Electronアプリケーション内でNext.jsを使用して多くの構成を回避し、Next.js routerをViewとして使用し、server-renderを使用してアプリケーションの初期レンダリングを高速化する方法を示しています。 Next.jsレイヤーとElectronレイヤーはどちらもTypeScriptで記述され、ビルドプロセス中にJavaScriptにコンパイルされます。

| Part       | Source code (Typescript) | Builds (JavaScript) |
| ---------- | ------------------------ | ------------------- |
| Next.js    | `/renderer`              | `/renderer`         |
| Electron   | `/electron-src`          | `/main`             |
| Production |                          | `/dist`             |

開発では、HTTPサーバーを実行し、Next.jsにルーティングを処理させます。  
本番環境では、HTTPサーバーを実行する代わりに、 `next export`を使用してHTML静的ファイルを事前生成し、アプリで使用します。

**Next.jsを使用してElectronアプリを構築する方法に関する詳細なドキュメントは、[こちら](https://leo.im/2017/electron-next)にあります!**

## How to use

Sample を生成するには [`create-next-app`](https://github.com/vercel/next.js/tree/canary/packages/create-next-app) を [npm](https://docs.npmjs.com/cli/init) もしくは [Yarn](https://yarnpkg.com/lang/en/docs/cli/create/) で実行します:

```bash
npx create-next-app --example with-electron-typescript with-electron-typescript-app
# or
yarn create next-app --example with-electron-typescript with-electron-typescript-app
```

使用可能なコマンド:

```bash
"yarn build-renderer": Next.jsレイヤーをトランスパイルする
"yarn build-electron": electronレイヤーをトランスパイルする
"yarn build": 両方のレイヤーをビルドする
"yarn start": 開発バージョンを開始
"yarn dist": プロダクションelctronビルドを作成する
"yarn type-check": TypeScriptでproject をcheck する
```

## Notes

`npm rundist` を使用して本番アプリを作成できます。

_タイプに関する注意:_

- Electronは独自の型定義を提供するため、@ types / electronをインストールする必要はありません。
  source: https://www.npmjs.com/package/@types/electron
- この例の作成時点では、「electron-next」に使用できるタイプはありませんでした。したがって、使用できるようになるまで、「electron-src」ディレクトリに「electron-next.d.ts」ファイルがあります。
