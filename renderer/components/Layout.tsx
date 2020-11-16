import React, { ReactNode } from 'react'
import Link from 'next/link'
import Head from 'next/head'
import SideBar from './SideBar';

type Props = {
  children: ReactNode
  title?: string
}

const Layout = ({ children, title = 'This is the default title' }: Props) => (
  <div>
    <Head>
      <title>{title}</title>
      <meta charSet="utf-8" />
      <meta name="viewport" content="initial-scale=1.0, width=device-width" />
      <script src="https://kit.fontawesome.com/a076d05399.js"></script>
    </Head>

    <SideBar header="Three App">
      <ul>
          <li><Link href="/"><a><i aria-hidden className="fas fa-qrcode">Main page</i></a></Link></li>
          <li><Link href="/three-area"><a><i aria-hidden className="fas fa-link">Force sensor view</i></a></Link></li>
      </ul>
    </SideBar>

    <main>
      {children}
    </main>

    <footer>
      <hr />
      <span>I'm here to stay (Footer)</span>
    </footer>
  </div>
)

export default Layout
