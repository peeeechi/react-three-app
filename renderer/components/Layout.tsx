import React, { ReactNode } from 'react'
import Link from 'next/link'
import Head from 'next/head'

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

    <input type="checkbox" id="check"/>
    <label htmlFor="check">
      <i className="fas fa-bars" id="btn"></i>
      <i className="fas fa-times" id="cancel"></i>
    </label>
    <div className="sidebar">
      <header>My App</header>
      <ul>
        <li><a href="#"><i className="fas fa-qrcode">Dashboard</i></a></li>
        <li><a href="#"><i className="fas fa-link">Shortcuts</i></a></li>
        <li><a href="#"><i className="fas fa-stream">Overview</i></a></li>
        <li><a href="#"><i className="fas fa-calendar-week">Events</i></a></li>
        <li><a href="#"><i className="fas fa-question-circle">About</i></a></li>
        <li><a href="#"><i className="fas fa-sliders-h">Services</i></a></li>
        <li><a href="#"><i className="fas fa-envelope">Contacts</i></a></li>
      </ul>
    </div>

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
