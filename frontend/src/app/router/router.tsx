import { createBrowserRouter } from 'react-router-dom';
import { MainLayout } from '@/widgets/layout';
import { HomePage } from '@/pages/home';
import { ProfilePage } from '@/pages/profile';

export const router = createBrowserRouter([
  {
    path: '/',
    element: <MainLayout />,
    children: [
      { index: true, element: <HomePage /> },
      { path: 'profile', element: <ProfilePage />, handle: { hideSidebar: true } },
    ],
  },
]);
