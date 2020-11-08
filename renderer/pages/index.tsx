import Link from 'next/link'
import Layout from '../components/Layout'

const IndexPage = () => {
  return (
    <div>
      <div className="hero">
        <h1 className="title">Next.js + Tailwind CSS 🐼</h1>
        <p className="text-center text-teal-500 text-2xl py-4">This is an Example.</p>
      </div>
    </div>

    // <Layout title="Home | Next.js + TypeScript + Electron Example">
    //   <h1>Hello Next.js 👋</h1>
    //   <p>
    //     <Link href="/about">
    //       <a>About</a>
    //     </Link>
    //   </p>
    // </Layout>
  )
}

export default IndexPage
